using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading;
using Lanit.Norma.AppServer.Resources;
using Lanit.Norma.AppServer.Security;
using System.Diagnostics;
using GotDotNet.ApplicationBlocks.Data;

namespace Lanit.Norma.AppServer
{
	/// <summary>
	/// Предоставляет доступ к функциональности транзакций System.Transactions
	/// </summary>
	public class Tx
	{
		#region Поля
		
		/// <summary>
		/// Уровень изоляции
		/// </summary>
		private readonly IsolationLevel _txLevel;

		/// <summary>
		/// Таймаут транзакции
		/// </summary>
		private readonly TimeSpan _txTimeout;

		/// <summary>
		/// Поведение при задании текущей транзакции
		/// </summary>
		private readonly TransactionScopeOption _tsOption;

		/// <summary>
		/// Текущий скоп
		/// </summary>
		private TransactionScope _ts;

		/// <summary>
		/// Флаг завершённости транзакции
		/// </summary>
		private volatile bool _txIsCompleted;

		/// <summary>
		/// Флаг, указывающий на то, что была создана новая корневая транзакция
		/// </summary>
		private bool _newTxCreated;

		/// <summary>
		/// Ссылка на транзакцию, действовавшую до создания нового контекста транзакции
		/// </summary>
		private Transaction _oldTx;

		/// <summary>
		/// Если Tx выводит блок кода из транзакции, необходимо воссоздать стандартный контекст соединений,
		/// чтобы соединения выбирались из пула по требованию
		/// </summary>
		private IDisposable _contextForSuppressOption;

		/// <summary>
		/// Если Tx вводит блок кода в транзацию, необходимо создать контекст соединений,
		/// обеспечивающий единственное соединение на транзакцию, взятое из пула.
		/// </summary>
		private IDisposable _contextForTxOption;

		/// <summary>
		/// Обработчики, которые должны выполниться в рамках транзакции после того, как будет завершён основной блок кода
		/// </summary>
		private static Dictionary<string, TxCompletionHandlerList> _beforeCompletionHandlers = new Dictionary<string, TxCompletionHandlerList>();

		/// <summary>
		/// Обработчики, которые должны выполниться за пределами транзакции после того,
		/// как будет завершён основной блок кода и закрыта сама транзация
		/// </summary>
		private static Dictionary<string, TxCompletionHandlerList> _afterCompletionHandlers = new Dictionary<string, TxCompletionHandlerList>();

		#endregion

		#region Свойства

		/// <summary>
		/// Возвращает флаг наличия транзакции
		/// </summary>
		public static bool IsInTx
		{
			get
			{
				return Transaction.Current != null;
			}
		}

		/// <summary>
		/// Токен текущей транзакции, используемый для маршаллинга
		/// Если транзакции нет, возвращается null
		/// </summary>
		public static byte[] TransactionToken
		{
			get
			{
				return Transaction.Current == null ? null : TransactionInterop.GetTransmitterPropagationToken(Transaction.Current);
			}
		}

		#endregion

		#region Required

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию с бесконечным таймаутом в режиме Read Committed
		/// </summary>
		/// <param name="action">Код, который необходимо выполнить внутри транзакции</param>
		public static void Required(Action action)
		{
			Required(TimeSpan.Zero, IsolationLevel.ReadCommitted, action);
		}

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию с выбранным таймаутом в режиме Read Committed
		/// </summary>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="action">Код, который необходимо выполнить внутри транзакции</param>
		public static void Required(TimeSpan timeout, Action action)
		{
			Required(timeout, IsolationLevel.ReadCommitted, action);
		}

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию
		/// </summary>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="level">Уровень изоляции транзакции</param>
		/// <param name="action">Код, который необходимо выполнить внутри транзакции</param>
		public static void Required(TimeSpan timeout, IsolationLevel level, Action action)
		{
			Tx tx = new Tx(TransactionScopeOption.Required, level, timeout);
			tx.ExecuteAction(action);
		}

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию с бесконечным таймаутом в режиме Read Committed
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого результата</typeparam>
		/// <param name="func">Код, который необходимо выполнить внутри транзакции</param>
		public static T Required<T>(Func<T> func)
		{
			return Required(TimeSpan.Zero, IsolationLevel.ReadCommitted, func);
		}

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию с выбранным таймаутом в режиме Read Committed
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого результата</typeparam>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="func">Код, который необходимо выполнить внутри транзакции</param>
		/// <returns>Результат</returns>
		public static T Required<T>(TimeSpan timeout, Func<T> func)
		{
			return Required(timeout, IsolationLevel.ReadCommitted, func);
		}

		/// <summary>
		/// Создаёт новую или использует текущую транзакцию
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого результата</typeparam>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="level">Уровень изоляции транзакции</param>
		/// <param name="func">Код, который необходимо выполнить внутри транзакции</param>
		/// <returns>Результат</returns>
		public static T Required<T>(TimeSpan timeout, IsolationLevel level, Func<T> func)
		{
			Tx tx = new Tx(TransactionScopeOption.Required, level, timeout);
			return tx.ExecuteFunc(func);
		}

		#endregion Required

		#region RequiresNew

		/// <summary>
		/// Создаёт новую транзакцию
		/// </summary>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="level">Уровень изоляции транзакции</param>
		/// <param name="action">Код, который необходимо выполнить внутри транзакции</param>
		public static void RequiresNew(TimeSpan timeout, IsolationLevel level, Action action)
		{
			Tx tx = new Tx(TransactionScopeOption.RequiresNew, level, timeout);
			tx.ExecuteAction(action);
		}

		/// <summary>
		/// Создаёт новую транзакцию
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого результата</typeparam>
		/// <param name="timeout">Таймаут транзакции</param>
		/// <param name="level">Уровень изоляции транзакции</param>
		/// <param name="func">Код, который необходимо выполнить внутри транзакции</param>
		/// <returns>Результат</returns>
		public static T RequiresNew<T>(TimeSpan timeout, IsolationLevel level, Func<T> func)
		{
			Tx tx = new Tx(TransactionScopeOption.RequiresNew, level, timeout);
			return tx.ExecuteFunc(func);
		}

		#endregion RequiresNew

		#region Suppress

		/// <summary>
		/// Выполняет код вне транзакции
		/// </summary>
		/// <param name="action">Код, который необходимо выполнить вне транзакции</param>
		public static void Suppress(Action action)
		{
			Tx tx = new Tx(TransactionScopeOption.Suppress);
			tx.ExecuteAction(action);
		}

		/// <summary>
		/// Выполняет код вне транзакции
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого результата</typeparam>
		/// <param name="func">Код, который необходимо выполнить вне транзакции</param>
		/// <returns>Результат</returns>
		public static T Suppress<T>(Func<T> func)
		{
			Tx tx = new Tx(TransactionScopeOption.Suppress);
			return tx.ExecuteFunc(func);
		}

		#endregion

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="tsOption">Поведение при задании текущей транзакции</param>
		private Tx(TransactionScopeOption tsOption)
		{
			this._tsOption = tsOption;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="tsOption">Поведение при задании текущей транзакции</param>
		/// <param name="txLevel">Уровень изоляции транзакции</param>
		/// <param name="txTimeout">Таймаут транзакции</param>
		private Tx(TransactionScopeOption tsOption, IsolationLevel txLevel, TimeSpan txTimeout)
		{
			// Если текущая транзакцию имеет уровень Snapshot, мы не можем поменять этот уровень
			this._txLevel = (Tx.IsInTx && Transaction.Current.IsolationLevel == IsolationLevel.Snapshot ? IsolationLevel.Snapshot : txLevel);
			this._txTimeout = txTimeout;
			this._tsOption = tsOption;
		}

		#endregion

		#region Методы, выполняющие транзакционный код

		private void ExecuteAction(Action action)
		{
			Transaction oldTx = Transaction.Current;
			try
			{
				_oldTx = oldTx;
				this.BeginTransaction();
				Transaction newTx = Transaction.Current;

				action();
				this.CommitTransaction();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
				Exception newException = this.OnException(ex);
				if (newException == null)
				{
					throw;
				}
				else
				{
					Trace.WriteLine(newException.ToString());
					throw newException;
				}
			}
			finally
			{
				try
				{
					this.DisposeTransaction();
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.ToString());
					Exception newException = this.OnException(ex);
					if (newException == null)
					{
						throw;
					}
					else
					{
						Trace.WriteLine(newException.ToString());
						throw newException;
					}
				}
			}
		}

		private T ExecuteFunc<T>(Func<T> func)
		{
			Transaction oldTx = Transaction.Current;
			try
			{
				_oldTx = oldTx;
				this.BeginTransaction();
				Transaction newTx = Transaction.Current;

				T result = func();
				this.CommitTransaction();
				return result;
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
				Exception newException = this.OnException(ex);
				if (newException == null)
				{
					throw;
				}
				else
				{
					Trace.WriteLine(newException.ToString());
					throw newException;
				}
			}
			finally
			{
				try
				{
					this.DisposeTransaction();
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.ToString());
					Exception newException = this.OnException(ex);
					if (newException == null)
					{
						throw;
					}
					else
					{
						Trace.WriteLine(newException.ToString());
						throw newException;
					}
				}
			}
		}

		#endregion

		#region Инфраструктурный код

		private void BeginTransaction()
		{
			if (this._tsOption == TransactionScopeOption.RequiresNew || this._tsOption == TransactionScopeOption.Required)
			{
				TransactionOptions to = new TransactionOptions()
				{
					IsolationLevel = this._txLevel,
					Timeout = this._txTimeout
				};

				Transaction oldTx = Transaction.Current;
				this._ts = new TransactionScope(this._tsOption, to);
				this._newTxCreated = oldTx != Transaction.Current;

				if (this._newTxCreated)
				{
					// Если создана новая транзакция, а текущий контекст соединений предполагает использование единственного соединения,
					// это единственное соединение должно войти в транзакцию
					ConnectionContext.NotifyOnTx(Transaction.Current);
				}

				if (!ConnectionContext.IsSingleConnectionContext)
				{
					// Если текущий контекст соединений не является контекстом единственного соединения,
					// назначается контекст единственного соединения
					_contextForTxOption = clsData.AcquireSingleConnectionContent(true);
				}

				// Обрабатываем событие завершения транзакции, которое вызывается в момент вызова TransactionScope.Dispose()
				Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(_tx_TransactionCompleted);
			}
			else
			{
				this._ts = new TransactionScope(this._tsOption);
				if (ConnectionContext.IsSingleConnectionContext)
				{
					// Если выводим блок кода из транзакции, принудительно устанавливаем стандартный контекст соединений
					_contextForSuppressOption = clsData.AcquireStandandConnectionContent();
				}
			}
		}

		void _tx_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			this._txIsCompleted = true;
			TransactionCompletedImpl(new TxCompletedEventArgs(e.Transaction, true), _afterCompletionHandlers);
		}

		void BeforeTransactionCompleted()
		{
			if (!IsInTx)
			{
				return;
			}
			TransactionCompletedImpl(new TxCompletedEventArgs(Transaction.Current, false), _beforeCompletionHandlers);
		}

		private static void TransactionCompletedImpl(TxCompletedEventArgs e, Dictionary<string, TxCompletionHandlerList> handlersCollection)
		{
			try
			{
				string txId = e.Tx.TransactionInformation.LocalIdentifier;
				TxCompletionHandlerList handlerList;
				lock (handlersCollection)
				{
					handlersCollection.TryGetValue(txId, out handlerList);
				}

				if (handlerList != null)
				{
					handlerList.TransactionCompleted(e);

					lock (handlersCollection)
					{
						handlersCollection.Remove(txId);
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
				throw;
			}
		}

		/// <summary>
		/// Завершает транзакцию
		/// </summary>
		private void CommitTransaction()
		{
			if (this._ts != null)
			{
				if (this._newTxCreated)
				{
					this.BeforeTransactionCompleted();
				}

				this._ts.Complete();
			}
		}

		/// <summary>
		/// Обработка ситуации, когда исключение может быть вызвано превышением таймаута
		/// </summary>
		private Exception OnException(Exception ex)
		{
			if ((ex is InvalidOperationException || ex is TransactionException || ex is TimeoutException) && this._tsOption != TransactionScopeOption.Suppress &&
				this._newTxCreated && Tx.IsInTx && Transaction.Current.TransactionInformation.Status == TransactionStatus.Aborted && this._txIsCompleted)
			{
				return new System.TimeoutException(TxResources.TimeoutMsg, ex);
			}

			return null;
		}

		private void DisposeTransaction()
		{
			if (this._ts != null)
			{
				if (this._tsOption != TransactionScopeOption.Suppress && _contextForTxOption == null)
				{
					// Обработчики события "после завершения транзакции" выполняется в стандартном контексте соединений
					using (clsData.AcquireStandandConnectionContent())
					{
						this._ts.Dispose();
					}
				}
				else
				{
					try
					{
						// Обработчики события "после завершения транзакции" выполняется в стандартном контексте соединений
						using (clsData.AcquireStandandConnectionContent())
						{
							this._ts.Dispose();
						}
					}
					finally
					{
						if (this._contextForSuppressOption != null)
						{
							this._contextForSuppressOption.Dispose();
							this._contextForSuppressOption = null;
						}

						if (this._contextForTxOption != null)
						{
							this._contextForTxOption.Dispose();
							this._contextForTxOption = null;
						}
					}
				}

				this._ts = null;
			}
		}

		#endregion

		#region Класс обработчика после завершения транзакции

		/// <summary>
		/// Класс обработчика после завершения транзакции
		/// </summary>
		private class TxCompletionHandlerList
		{
			private LinkedList<ITxCompletionHandler> _handlers = new LinkedList<ITxCompletionHandler>();

			/// <summary>
			/// Конструктор
			/// </summary>
			public TxCompletionHandlerList()
			{
			}

			public void TransactionCompleted(TxCompletedEventArgs e)
			{
				// Реализация цикла должна быть именно такой.
				// Причина: набор обработчиков может расширяться по мере выполнения этого цикла
				while (true)
				{
					ITxCompletionHandler handler;
					lock (this._handlers)
					{
						if (this._handlers.Count == 0)
						{
							break;
						}
						
						handler = this._handlers.First.Value;
					}

					bool cycleIsTerminated = false;
					try
					{
						handler.TxCompleted(e);
					}
					catch (Exception ex)
					{
						Trace.WriteLine(ex.ToString());

						// Если ошибка возникает в обрабочике внутри транзакции, исключение должно быть вброшено
						if (!e.IsAfterTxCompleted)
						{
							cycleIsTerminated = true;
							throw;
						}
					}
					finally
					{
						if (!cycleIsTerminated)
						{
							this._handlers.RemoveFirst();
						}
					}
				}
			}

			/// <summary>
			/// Добавляет обработчик
			/// </summary>
			public void AddHandler(ITxCompletionHandler handler)
			{
				lock (this._handlers)
				{
					IExtensibleTxCompletionHandler extensibleHandler = handler as IExtensibleTxCompletionHandler;
					if (extensibleHandler != null)
					{
						foreach (ITxCompletionHandler addedHandler in this._handlers)
						{
							// Расширяемые обработчики завершения транзакции не добавляются в список обработчиков,
							// если в списке есть обработчик, действие которого они могут расширить
							IExtensibleTxCompletionHandler addedExtensibleHandler = addedHandler as IExtensibleTxCompletionHandler;
							if (addedExtensibleHandler != null && addedExtensibleHandler.Extend(extensibleHandler))
							{
								return;
							}
						}
					}

					this._handlers.AddLast(handler);
				}
			}
		}

		#endregion

		#region Поддержка реакции на завершение транзакции


		/// <summary>
		/// Добавляет обработчик после завершения транзакции
		/// </summary>
		public static void AddCompletionHandler(ITxCompletionHandler handler)
		{
			AddCompletionHandler(handler, true);
		}


		/// <summary>
		/// Добавляет обработчик до завершения транзакции
		/// </summary>
		public static void AddBeforeCompletionHandler(ITxCompletionHandler handler)
		{
			AddCompletionHandler(handler, false);
		}


		/// <summary>
		/// Добавляет обработчик завершения транзакции
		/// </summary>
		private static void AddCompletionHandler(ITxCompletionHandler handler, bool isAfterTxCompleted)
		{
			if (!IsInTx)
			{
				// Если в момент задания обработчика транзакции нет, обработчик выполняется сразу же
				handler.TxCompleted(new TxCompletedEventArgs(null, isAfterTxCompleted));
			}
			else
			{
				// Если в момент задания обработчика есть транзакция, обработчик ожидает завершения транзакции
				string txId = Transaction.Current.TransactionInformation.LocalIdentifier;
				
				// Логика исключает возникновение дедлока, который способен привести к катастрофическим последствиям
				TxCompletionHandlerList handlerList = null;
				Dictionary<string, TxCompletionHandlerList> handlerCollection = (isAfterTxCompleted ? _afterCompletionHandlers : _beforeCompletionHandlers);
				lock (handlerCollection)
				{
					if (!handlerCollection.TryGetValue(txId, out handlerList))
					{
						handlerList = new TxCompletionHandlerList();
						handlerCollection.Add(txId, handlerList);
					}
				}

				handlerList.AddHandler(handler);
			}
		}

		#endregion
	}
}
