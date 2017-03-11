namespace LogEngine
{
    /// <summary>
    /// Тип журнала
    /// </summary>
    public enum LogType : int
    {
        /// <summary>
        /// Системный журнал, не связанный с действиями пользователя
        /// </summary>
        System = 0,

        /// <summary>
        /// Журнал операций
        /// </summary>
        Operations = 1
    }
}
