using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Maxima
{
    public static class SqlConnectionExtensions
    {
        public static bool SafeOpen(this SqlConnection connection)
        {
            try
            {
                connection.Open();
            }
            catch (SqlException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> SafeOpenAsync(this SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (SqlException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Метод ассинхронного открытия подключения к БД. 
        /// </summary>
        /// <remarks>Стандартный метод <see cref="SqlConnection.OpenAsync"/> блокирует выполнение потока при недоступности сервера.</remarks>
        public static Task OpenAsyncEx(this SqlConnection connection)
        {
            return Task.Factory.StartNew(connection.Open);
        }
    }
}
