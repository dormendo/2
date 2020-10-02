using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Lanit.Norma.Exceptions;
using System.Threading;

namespace Lanit.Norma.Marshal
{
	/// <summary>
	/// Бизнес-процесс
	/// </summary>
	[Serializable]
	public abstract class BizProcess : ISerializable
	{
		#region Поля, задающие поведение процесса и механизмов его обработки, отражающие состояние процесса
		
		/// <summary>
		/// Статус процесса
		/// </summary>
		private BpStatus _status = BpStatus.Created;

		/// <summary>
		/// Этап жизненного цикла процесса
		/// </summary>
		public BpStage Stage = BpStage.Created;

		/// <summary>
		/// Флаг пропуска всех оставшихся фаз
		/// </summary>
		public bool CompleteExecution = false;

		/// <summary>
		/// Флаг, указывающий на то, что процесс был запущен на стороне клиентского приложения
		/// </summary>
		public bool? StartedOnServer;

		/// <summary>
		/// Контекст, в котором процесс был создан
		/// </summary>
		private ContextData _contextData;

		/// <summary>
		/// Флаг, указывающий на необходимость немедленного вброса любых возникающих исключений
		/// </summary>
		public bool PopulateExceptions = true;

		/// <summary>
		/// Дата-время начала процесса
		/// </summary>
		public DateTime StartTime;

		/// <summary>
		/// Дата-время окончания процесса
		/// </summary>
		public DateTime FinishTime;

		#endregion

		#region Системные свойства процесса

		/// <summary>
		/// Идентификатор типа процесса
		/// </summary>
		public abstract string ProcessTypeId
		{
			get;
		}

		/// <summary>
		/// Идентификатор процесса
		/// </summary>
		public Guid Id
		{
			get;
			protected set;
		}

		/// <summary>
		/// Параметры процесса
		/// </summary>
		protected Dictionary<string, object> Parameters
		{
			get;
			set;
		}

		/// <summary>
		/// Родительский процесс
		/// </summary>
		public BizProcess ParentProcess
		{
			get;
			set;
		}

		/// <summary>
		/// Дочерние процессы
		/// </summary>
		public List<BizProcess> ChildProcesses
		{
			get;
			protected set;
		}

		/// <summary>
		/// Результат выполнения процесса.
		/// </summary>
		public BpResultData Result
		{
			get;
			set;
		}

		/// <summary>
		/// Текущий прогресс выполнения процесса
		/// </summary>
		public BpProgressData Progress
		{
			get;
			protected set;
		}

		/// <summary>
		/// Флаг корневого процесса
		/// </summary>
		public bool IsRoot
		{
			get
			{
				return this.ParentProcess == null;
			}
		}

		/// <summary>
		/// Контекст, в котором процесс был запущен на выполнение
		/// </summary>
		public ContextData ContextData
		{
			get
			{
				return this._contextData;
			}
		}

		/// <summary>
		/// Режим слежения за ходом выполнения процесса
		/// </summary>
		public BpTrackMode TrackMode
		{
			get;
			set;
		}

		/// <summary>
		/// Статус процесса
		/// </summary>
		public BpStatus Status
		{
			get
			{
				return this._status;
			}
			set
			{
				if (value == BpStatus.Completed && this._status != BpStatus.Completed)
				{
					this.FinishTime = DateTime.Now;
				}
				this._status = value;
			}
		}

		/// <summary>
		/// Информация об узле, на котором выполняется бизнес-процесс
		/// </summary>
		public BpRunningNode Node
		{
			get;
			set;
		}

		#endregion
		
		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		protected BizProcess()
		{
			this.Id = Guid.NewGuid();
			this.TrackMode = BpTrackMode.None;
			this.Parameters = new Dictionary<string, object>();
			this.Result = this.GetResultInstance();
			this.Progress = this.GetProgressInstance();
			this.ChildProcesses = new List<BizProcess>();
			this._contextData = ProcessDispatcher.Instance.GetContextData();
			this.Node = ProcessDispatcher.Instance.NodeInformation;
		}

		#endregion

		#region Методы управления деревом процессов

		/// <summary>
		/// Возвращает корневой процесс дерева процессов
		/// </summary>
		/// <returns></returns>
		public BizProcess GetRoot()
		{
			return (this.ParentProcess == null ? this : this.ParentProcess.GetRoot());
		}

		/// <summary>
		/// Возвращает из поддерева процесс по его идентификатору
		/// </summary>
		/// <param name="id">Идентификатор процесса</param>
		/// <returns>Процесс</returns>
		public BizProcess GetById(Guid id)
		{
			return GetByIdInternal(this, id);
		}

		/// <summary>
		/// Возвращает из поддерева процесс по его идентификатору и приводит к указанному типу
		/// </summary>
		/// <typeparam name="BP">Тип, к которому нужно привести найденный процесс</typeparam>
		/// <param name="id">Идентификатор процесса</param>
		/// <returns>Процесс</returns>
		public BP GetById<BP>(Guid id) where BP : BizProcess
		{
			return this.GetById(id) as BP;
		}

		/// <summary>
		/// Возвращает из поддерева процесс по его идентификатору
		/// </summary>
		/// <param name="process">Процесс, среди подпроцессов которого осуществляется поиск</param>
		/// <param name="id">Идентификатор процесса</param>
		/// <returns>Процесс</returns>
		private static BizProcess GetByIdInternal(BizProcess process, Guid id)
		{
			if (process.Id == id)
			{
				return process;
			}

			lock (process.ChildProcesses)
			{
				foreach (BizProcess child in process.ChildProcesses)
				{
					BizProcess foundProcess = GetByIdInternal(child, id);
					if (foundProcess != null)
					{
						return foundProcess;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Добавляет дочерний процесс
		/// </summary>
		/// <param name="child">Дочерний процесс</param>
		/// <returns>Дочерний процесс</returns>
		public void AddChild(BizProcess child)
		{
			lock (this.ChildProcesses)
			{
				child.ParentProcess = this;
				this.ChildProcesses.Add(child);
			}
		}

		/// <summary>
		/// Возвращает первого родителя заданного типа из цепочки родительских процессов
		/// </summary>
		/// <typeparam name="BP">Тип бизнес-процесса</typeparam>
		/// <returns>Типизированный бизнес-процесс или null, если процесс заданного типа не найден</returns>
		public BP GetParentOfType<BP>() where BP : class
		{
			if (this.ParentProcess == null)
			{
				return default(BP);
			}
			BP cast = this.ParentProcess as BP;
			if (cast != null)
			{
				return cast;
			}
			return this.ParentProcess.GetParentOfType<BP>();
		}

		#endregion

		#region Методы работы с параметрами процессов

		/// <summary>
		/// Установка значения параметра
		/// </summary>
		/// <param name="parameterKey">Ключ параметра</param>
		/// <param name="parameterValue">Значение параметра</param>
		public void SetParameter(string parameterKey, object parameterValue)
		{
			lock (this.Parameters)
			{
				this.Parameters[parameterKey] = parameterValue;
			}
		}

		/// <summary>
		/// Получение значения параметра
		/// </summary>
		/// <param name="parameterKey">Ключ параметра</param>
		/// <returns>Значение параметра, если параметр включен в коллекцию. Иначе null</returns>
		public object GetParameter(string parameterKey)
		{
			lock (this.Parameters)
			{
				object value;
				this.Parameters.TryGetValue(parameterKey, out value);
				return value;
			}
		}

		/// <summary>
		/// Получение значения параметра
		/// </summary>
		/// <param name="parameterKey">Ключ параметра</param>
		/// <returns>Значение параметра, если параметр включен в коллекцию. Иначе null</returns>
		public T GetParameter<T>(string parameterKey)
		{
			T value;
			this.TryGetParameter<T>(parameterKey, out value);
			return value;
		}

		/// <summary>
		/// Получение значения параметра
		/// </summary>
		/// <param name="parameterKey">Ключ параметра</param>
		/// <param name="value">Значение параметра</param>
		/// <returns>true, если параметр найден и может быть преобразован в заданный тип. Иначе false</returns>
		public bool TryGetParameter<T>(string parameterKey, out T value)
		{
			lock (this.Parameters)
			{
				object oValue;
				bool result = this.Parameters.TryGetValue(parameterKey, out oValue);
				value = (result && oValue is T ? (T)oValue : default(T));
				return result;
			}
		}

		#endregion

		#region Методы управления процессами

		/// <summary>
		/// Выполняет процесс
		/// </summary>
		/// <returns>Результат выполнения процесса</returns>
		public BizProcess Run()
		{
			if (!this.StartedOnServer.HasValue)
			{
				this.StartedOnServer = ProcessDispatcher.Instance.IsServerSide;
			}
			return ProcessDispatcher.Instance.RunProcess(this);
		}

		/// <summary>
		/// Запустить процесс на выполнение
		/// </summary>
		/// <typeparam name="T">Тип, к которому необходимо привести результат выполнения процесса</typeparam>
		/// <returns>Результат выполнения процесса приведенный к типу <typeparamref name="T"/></returns>
		public T Run<T>() where T : BizProcess
		{
			return (T)this.Run();
		}

		/// <summary>
		/// Запускает процесс на выполнение в асинхронном режиме
		/// </summary>
		/// <returns>Информация о запущенном процессе</returns>
		public BpExecuteState RunAsync()
		{
			if (!this.StartedOnServer.HasValue)
			{
				this.StartedOnServer = ProcessDispatcher.Instance.IsServerSide;
			}
			return ProcessDispatcher.Instance.RunProcessAsync(this);
		}

		#endregion

		#region Методы для работы с данными о прогрессе и результате выполнения

		/// <summary>
		/// Возвращает экземпляр типа результата
		/// </summary>
		/// <returns>Экземпляр типа результата</returns>
		public virtual BpResultData GetResultInstance()
		{
			return new BpResultData();
		}

		/// <summary>
		/// Возвращает результат, приведённый к заданному типу
		/// </summary>
		/// <typeparam name="T">Тип, к которому нужно привести результат</typeparam>
		/// <returns>Результат, приведённый к заданному типу</returns>
		public virtual T GetResult<T>() where T : BpResultData
		{
			return (T)this.Result;
		}

		/// <summary>
		/// Возвращает экземпляр типа данных о прогрессе
		/// </summary>
		/// <returns>Экземпляр типа данных о прогрессе</returns>
		public virtual BpProgressData GetProgressInstance()
		{
			return new BpProgressData();
		}

		/// <summary>
		/// Возвращает данные о прогрессе, приведённые к заданному типу
		/// </summary>
		/// <typeparam name="T">Тип, к которому нужно привести данные о прогрессе</typeparam>
		/// <returns>Данные о прогрессе, приведённые к заданному типу</returns>
		public T GetProgress<T>() where T : BpProgressData
		{
			return (T)this.Progress;
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		/// <param name="percent">Процент выполнения операции</param>
		public void SetProgressForPercentBased(string stageId, decimal percent)
		{
			SetProgress(stageId, ProcessStageType.PercentBased, 0, 0, percent, TimeSpan.Zero);
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		/// <param name="processedCount">Количество обработанных элементов</param>
		/// <param name="totalCount">Общее количество элементов</param>
		public void SetProgressForProgressEnabled(string stageId, decimal processedCount, decimal totalCount)
		{
			SetProgress(stageId, ProcessStageType.ProgressEnabled, processedCount, totalCount, 0, TimeSpan.Zero);
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		/// <param name="processedCount">Количество обработанных элементов</param>
		/// <param name="totalCount">Общее количество элементов</param>
		/// <param name="span">Временной интервал</param>
		public void SetProgressForProgressEnabled(string stageId, decimal processedCount, decimal totalCount, TimeSpan span)
		{
			SetProgress(stageId, ProcessStageType.ProgressEnabled, processedCount, totalCount, 0, span);
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		public void SetProgressForShortTerm(string stageId)
		{
			SetProgress(stageId, ProcessStageType.ShortTerm, 0, 0, 0, TimeSpan.Zero);
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		public void SetProgressForLongTerm(string stageId)
		{
			SetProgress(stageId, ProcessStageType.LongTerm, 0, 0, 0, TimeSpan.Zero);
		}

		/// <summary>
		/// Устанавливает прогресс выполнения операции
		/// </summary>
		/// <param name="stageId">Идентификатор операции</param>
		/// <param name="stageType">Тип операции</param>
		/// <param name="processedCount">Количество обработанных элементов</param>
		/// <param name="totalCount">Общее количество элементов</param>
		/// <param name="percent">Процент выполнения операции</param>
		/// <param name="span">Временной интервал</param>
		protected void SetProgress(string stageId, ProcessStageType stageType, decimal processedCount,
			decimal totalCount, decimal percent, TimeSpan span)
		{
			BpProgressData newProgress = this.Progress.Clone(BpCloneContext.Status);
			newProgress.SetProgress(stageId, stageType, processedCount, totalCount, percent, span);
			this.Progress = newProgress;
		}

		#endregion

		#region Клонирование, слияние деревьев процессов и выделение поддеревьев

		/// <summary>
		/// Клонирует дерево процесса, начиная с корня
		/// </summary>
		/// <param name="cloneContext">Контекст клонирования</param>
		/// <returns>Копия процесса</returns>
		public BizProcess CloneWithParents(BpCloneContext cloneContext)
		{
			return this.GetRoot().Clone(cloneContext).GetById(this.Id);
		}
		
		/// <summary>
		/// Клонирует поддерево процессов, начиная с текущего
		/// </summary>
		/// <param name="cloneContext">Контекст клонирования</param>
		/// <returns>Копия процесса</returns>
		public virtual BizProcess Clone(BpCloneContext cloneContext)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Конструктор. Используется только при клонировании
		/// </summary>
		/// <param name="origin">Оригинальный экземпляр</param>
		/// <param name="cloneContext">Контекст клонирования</param>
		protected BizProcess(BizProcess origin, BpCloneContext cloneContext)
		{
			this.CompleteExecution = origin.CompleteExecution;
			this.StartedOnServer = origin.StartedOnServer;
			
			this.Id = (cloneContext.IsPrototype() ? Guid.NewGuid() : origin.Id);
			
			this.Stage = origin.Stage;
			this._status = origin._status;
			this.StartTime = origin.StartTime;
			this.FinishTime = origin.FinishTime;

			if (!cloneContext.IsAnyPeristence())
			{
				this.Progress = origin.Progress;
			}

			this.ChildProcesses = new List<BizProcess>();
			if (!cloneContext.IsShortPeristence())
			{
				lock (origin.ChildProcesses)
				{
					foreach (BizProcess childProcess in origin.ChildProcesses)
					{
						this.AddChild(childProcess.Clone(cloneContext));
					}
				}
			}

			if (!cloneContext.IsAnyPeristence() || origin.ParentProcess == null)
			{
				this._contextData = origin._contextData;
			}

			this.Parameters = new Dictionary<string, object>();
			this.CloneParameters(origin, cloneContext);

			if (!cloneContext.IsStatus())
			{
				this.Result = origin.Result.Clone(cloneContext);
			}
			this.PopulateExceptions = origin.PopulateExceptions;
			this.TrackMode = origin.TrackMode;
			this.Node = origin.Node;
		}

		/// <summary>
		/// Клонирует значения параметров
		/// </summary>
		/// <param name="origin">Оригинальный экземпляр</param>
		/// <param name="cloneContext">Контекст клонирования</param>
		protected virtual void CloneParameters(BizProcess origin, BpCloneContext cloneContext)
		{
			if (!cloneContext.IsStatus())
			{
				lock (origin.Parameters)
				{
					foreach (KeyValuePair<string, object> pair in origin.Parameters)
					{
						this.Parameters.Add(pair.Key, pair.Value);
					}
				}
			}
		}

		#endregion

		#region ISerializable Members

		/// <summary>
		/// Cериализует процесс
		/// </summary>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				using (CustomBinaryWriter bw = new CustomBinaryWriter(ms))
				{
					this.SerializeInternal(bw);
				}
				info.AddValue("_", ms.ToArray());
			}
		}

		/// <summary>
		/// Сериализует процесс
		/// </summary>
		/// <param name="bw">Писатель в поток</param>
		protected virtual void SerializeInternal(CustomBinaryWriter bw)
		{
			bw.Write(this.CompleteExecution);
			bw.Write(this.Id);
			bw.Write(this.PopulateExceptions);
			bw.Write((byte)this.Stage);
			bw.WriteNullable(this.StartedOnServer);
			bw.Write((byte)this._status);
			bw.Write((byte)this.TrackMode);
			bw.Write(this.StartTime);
			bw.Write(this.FinishTime);

			bw.Write(this._contextData != null);
			if (this._contextData != null)
			{
				this._contextData.SerializeInternal(bw);
			}
			
			lock (this.Parameters)
			{
				bw.Write7BitEncodedInt(this.Parameters.Count);
				foreach (string key in this.Parameters.Keys)
				{
					bw.Write(key);
				}
				foreach (object value in this.Parameters.Values)
				{
					bw.WriteObject(value);
				}
			}

			//bw.WriteObject(this.Progress);
			//bw.WriteObject(this.Result);

			bw.Write(this.Progress != null);
			if (this.Progress != null)
			{
				((ICustomSerializable)this.Progress).SerializeInternal(bw);
			}

			bw.Write(this.Result != null);
			if (this.Result != null)
			{
				((ICustomSerializable)this.Result).SerializeInternal(bw);
			}

			lock (this.ChildProcesses)
			{
				bw.Write7BitEncodedInt(this.ChildProcesses.Count);
				foreach (BizProcess process in this.ChildProcesses)
				{
					this.WriteChildProcess(bw, process);
				}
			}

			bw.Write((byte)this.Node.NodeType);
			bw.Write(this.Node.NodeName);
		}

		/// <summary/>
		protected void WriteChildProcess(CustomBinaryWriter bw, BizProcess childProcess)
		{
			if (BizProcessHolder.Instance.GetType(childProcess.ProcessTypeId) != null)
			{
				bw.Write(true);
				bw.Write(childProcess.ProcessTypeId);
				childProcess.SerializeInternal(bw);
			}
			else
			{
				bw.Write(false);
				bw.WriteObject(childProcess);
			}
		}

		/// <summary>
		/// Десериализует процесс
		/// </summary>
		protected BizProcess(SerializationInfo info, StreamingContext context)
		{
			using (MemoryStream ms = new MemoryStream((byte[])info.GetValue("_", typeof(byte[]))))
			{
				using (CustomBinaryReader br = new CustomBinaryReader(ms))
				{
					this.DeserializeInternal(br);
				}
			}
		}

		/// <summary>
		/// Десериализует процесс
		/// </summary>
		/// <param name="br">Читатель из потока</param>
		protected virtual void DeserializeInternal(CustomBinaryReader br)
		{
			this.CompleteExecution = br.ReadBoolean();
			this.Id = br.ReadGuid();
			this.PopulateExceptions = br.ReadBoolean();
			this.Stage = (BpStage)br.ReadByte();
			this.StartedOnServer = br.ReadNullableBoolean();
			this._status = (BpStatus)br.ReadByte();
			this.TrackMode = (BpTrackMode)br.ReadByte();
			this.StartTime = br.ReadDateTime();
			this.FinishTime = br.ReadDateTime();

			if (br.ReadBoolean())
			{
				this._contextData = new ContextData();
				this._contextData.DeserializeInternal(br);
			}
			
			int count = br.Read7BitEncodedInt();
			List<string> paramNames = new List<string>(count);
			this.Parameters = new Dictionary<string, object>(count);
			for (int i = 0; i < count; i++)
			{
				paramNames.Add(br.ReadString());
			}
			for (int i = 0; i < count; i++)
			{
				this.Parameters.Add(paramNames[i], br.ReadObject());
			}

			//this.Progress = (BpProgressData)br.ReadObject();
			//this.Result = (BpResultData)br.ReadObject();

			if (br.ReadBoolean())
			{
				this.Progress = this.GetProgressInstance();
				((ICustomSerializable)this.Progress).DeserializeInternal(br);
			}

			if (br.ReadBoolean())
			{
				this.Result = this.GetResultInstance();
				((ICustomSerializable)this.Result).DeserializeInternal(br);
			}

			count = br.Read7BitEncodedInt();
			this.ChildProcesses = new List<BizProcess>(count);
			for (int i = 0; i < count; i++)
			{
				BizProcess process = this.ReadChildProcess(br);
				this.AddChild(process);
			}

			BpRunningNode.Type brnType = (BpRunningNode.Type)br.ReadByte();
			string brnName = br.ReadString();
			this.Node = new BpRunningNode(brnType, brnName);
		}

		/// <summary/>
		protected BizProcess ReadChildProcess(CustomBinaryReader br)
		{
			bool optimized = br.ReadBoolean();
			if (optimized)
			{
				string typeId = br.ReadString();
				Type type = BizProcessHolder.Instance.GetType(typeId);
				BizProcess childProcess = (BizProcess)Activator.CreateInstance(type, true);
				childProcess.DeserializeInternal(br);
				return childProcess;
			}
			else
			{
				return br.ReadObject<BizProcess>();
			}
		}

		#endregion

		#region Работа с текстовым представлением процесса

		/// <summary>
		/// Возвращает основное текстовое описание процесса
		/// </summary>
		/// <returns>Текстовое описание процесса</returns>
		public virtual string GetProcessDescription()
		{
			return this.ProcessTypeId;
		}

		/// <summary>
		/// Возвращает текстовое описание операции внутри процесса
		/// </summary>
		/// <returns>Текстовое описание операции внутри процесса</returns>
		public virtual string GetProcessOperationDescription()
		{
			if (this.Progress == null || string.IsNullOrEmpty(this.Progress.StageId))
			{
				return this.GetProcessDescription();
			}
			else
			{
				return this.Progress.StageId;
			}
		}

		#endregion

		#region Поддержка запросов по формальным данным

		/// <summary>
		/// Возвращает флаг поддержки запроса по формальным данным
		/// </summary>
		public virtual bool IsRequestable
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Проверяет соответствие формальным данным запроса
		/// </summary>
		/// <param name="requestData">Формальные данные запроса</param>
		/// <returns>true, если бизнес-процесс удовлетворяет данным запроса. Иначе false</returns>
		public virtual bool IsRequestMatched(BpRequestData requestData)
		{
			return false;
		}

		#endregion
	}
}
