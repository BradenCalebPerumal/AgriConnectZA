using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GreenAgriApp.Services
{
    public class FirebaseService
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly string _apiKey;

        public FirebaseService(IConfiguration config)
        {
            _apiKey = config["Firebase:ApiKey"];
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
        }

        public async Task<FirebaseAuthLink> SignInAsync(string email, string password)
        {
            try
            {
                return await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
            }
            catch
            {
                return null;
            }
        }

      public async Task<FirebaseAuthLink> RegisterAsync(string email, string password)
{
    try
    {
        return await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
    }
    catch (FirebaseAuthException ex)
    {
        // Log raw message if needed: Console.WriteLine(ex.Message);

        if (ex.Reason == AuthErrorReason.EmailExists)
        {
            throw new ApplicationException("This email is already registered.");
        }
        else
        {
            throw new ApplicationException("Firebase registration failed: " + ex.Reason.ToString());
        }
    }
}

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
