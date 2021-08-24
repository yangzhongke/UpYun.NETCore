using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UpYun.NETCore
{
    public class UpYunResult
    {
        public bool IsOK { get; set; }
        public string Msg { get; set; }
        public int Code { get; set; }
        public string Id { get; set; }

        public static readonly UpYunResult OK = new UpYunResult { IsOK=true};
        public static async Task<UpYunResult> CreateErrorAsync(HttpResponseMessage resp)
        {
            string body = await resp.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            UpYunResult result = JsonSerializer.Deserialize<UpYunResult>(body, options);
            result.IsOK = false;
            return result;
        }
    }

    public class UpYunResult<T>
    {
        public bool IsOK { get; set; }
        public string Msg { get; set; }
        public int Code {  get; set; }
        public string Id { get; set; }

        public T Value { get; set; }

        public static UpYunResult<T> OK(T value)
        {
            UpYunResult<T> result = new UpYunResult<T>();
            result.IsOK = true;
            result.Value = value;
            return result;
        }

        public static async Task<UpYunResult<T>> CreateErrorAsync(HttpResponseMessage resp)
        {
            string body = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UpYunResult<T>>(body);
            result.IsOK = false;
            return result;
        }

        public static implicit operator UpYunResult<T>(T value)
        {
            return new UpYunResult<T> { IsOK=true,Value=value};
        }
    }
}
