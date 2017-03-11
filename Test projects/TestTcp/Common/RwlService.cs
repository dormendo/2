using System;
using System.Threading;

namespace TestTcp
{
	/// <summary>
	/// Сервис для работы с ReaderWriterLockSlim без таймаутов ожидания
	/// </summary>
	public class RwlService : IDisposable
	{
		#region Поля

		private bool _isStarted;

		private bool _isFinished;

		private ReaderWriterLockSlim _lock;

		private LockType _lockType;

		private bool _noEscalate;

		#endregion

		public RwlService(ReaderWriterLockSlim rwlock, LockType lockType, bool noEscalate)
		{
			this._isStarted = false;
			this._isFinished = false;
			this._lock = rwlock;
			this._lockType = lockType;
			this._noEscalate = noEscalate;
		}

		#region Перечисление типов запрашиваемой блокировки
		/// <summary>
		/// Тип блокировки
		/// </summary>
		public enum LockType
		{
			/// <summary>
			/// Блокировка чтения
			/// </summary>
			Reader,

			/// <summary>
			/// Блокировка записи
			/// </summary>
			Writer,

			/// <summary>
			/// Блокировка чтения с возможностью эскалации до блокировки записи
			/// </summary>
			UpgradeableReader
		}

		#endregion

		#region Свойства

		private bool HeldNoLock
		{
			get
			{
				return !this._lock.IsReadLockHeld && !this._lock.IsWriteLockHeld && !this._lock.IsUpgradeableReadLockHeld;
			}
		}

		#endregion

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="rwlock">Примитив синхронизации</param>
		/// <param name="lockType">Тип блокировки</param>
		/// <param name="checkHeldLock">Флаг необходимости проверки установленных ранее блокировок</param>
		/// <param name="noEscalate">Флаг, запрещающий эскалацию блокировки</param>
		#region Получение экземпляра без проверок на наличие установленных ранее блокировок

		/// <summary>
		/// Возвращает экземпляр для работы с блокировкой чтения
		/// </summary>
		/// <param name="rwlock">Примитив синхронизации</param>
		/// <returns>Экземпляр сервиса</returns>
		public static RwlService Read(ReaderWriterLockSlim rwlock)
		{
			return Get(rwlock, LockType.Reader, true);
		}

		/// <summary>
		/// Возвращает экземпляр для работы с блокировкой чтения
		/// </summary>
		/// <param name="rwlock">Примитив синхронизации</param>
		/// <returns>Экземпляр сервиса</returns>
		public static RwlService ReadWithUpgrade(ReaderWriterLockSlim rwlock)
		{
			return Get(rwlock, LockType.UpgradeableReader, true);
		}

		/// <summary>
		/// Возвращает экземпляр для работы с блокировкой записи.
		/// Возможна эскалация блокировки, что при неправильной работе может приводить в дедлокам
		/// </summary>
		/// <param name="rwlock">Примитив синхронизации</param>
		/// <returns>Экземпляр сервиса</returns>
		public static RwlService Write(ReaderWriterLockSlim rwlock)
		{
			return Get(rwlock, LockType.Writer, false);
		}

		/// <summary>
		/// Возвращает экземпляр для работы с блокировкой записи.
		/// Эскалация блокировки невозможна, что может выражаться в невозможности получить блокировку записи
		/// </summary>
		/// <param name="rwlock">Примитив синхронизации</param>
		/// <returns>Экземпляр сервиса</returns>
		public static RwlService WriteNoEscalate(ReaderWriterLockSlim rwlock)
		{
			return Get(rwlock, LockType.Writer, true);
		}

		#endregion

		/// <summary>
		/// Начало выполнения метода
		/// </summary>
		public void Start()
		{
			if (this._lock == null)
			{
				return;
			}

			if (this._lockType == LockType.Reader)
			{
				this._lock.EnterReadLock();
				this._isStarted = true;
				return;
			}
			else if (this._lockType == LockType.Writer)
			{
				if (this.HeldNoLock || (!this._noEscalate && this._lock.IsUpgradeableReadLockHeld))
				{
					this._lock.EnterWriteLock();
					this._isStarted = true;
					return;
				}
			}
			else if (this._lockType == LockType.UpgradeableReader)
			{
				this._lock.EnterUpgradeableReadLock();
				this._isStarted = true;
				return;
			}
		}

		/// <summary>
		/// Окончание выполнения метода
		/// </summary>
		public void Finish()
		{
			if (this._isStarted && !this._isFinished)
			{
				if (this._lockType == LockType.Reader)
				{
					this._lock.ExitReadLock();
				}
				else if (this._lockType == LockType.Writer)
				{
					this._lock.ExitWriteLock();
				}
				else if (this._lockType == LockType.UpgradeableReader)
				{
					this._lock.ExitUpgradeableReadLock();
				}
			}

			this._isFinished = true;
		}

		/// <summary>
		/// Освобождение ресурсов
		/// </summary>
		public void Dispose()
		{
			if (!this._isFinished)
			{
				this.Finish();
			}
		}

		private static RwlService Get(ReaderWriterLockSlim rwlock, LockType lockType, bool noEscalate)
		{
			if (rwlock == null || rwlock.RecursionPolicy == LockRecursionPolicy.SupportsRecursion)
			{
				throw new InvalidOperationException("No lock or recursive lock");
			}

			var service = new RwlService(rwlock, lockType, noEscalate);
			service.Start();
			return service;
		}

		private bool HeldNoLockExcept(LockType lockType)
		{
			switch (lockType)
			{
				case LockType.Reader:
					return !this._lock.IsWriteLockHeld && !this._lock.IsUpgradeableReadLockHeld;
				case LockType.UpgradeableReader:
					return !this._lock.IsReadLockHeld && !this._lock.IsWriteLockHeld;
				case LockType.Writer:
					return !this._lock.IsReadLockHeld && !this._lock.IsUpgradeableReadLockHeld;
				default:
					return false;
			}
		}
	}
}