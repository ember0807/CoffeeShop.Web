using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CoffeeShop.Web.Extensions
{
    public static class SessionExtensions
    {
        // Метод для сохранения объекта в сессию
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Метод для получения объекта из сессии
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}