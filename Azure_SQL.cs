using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Identity.Client; // MSAL

namespace Console_Connect_SQL
{
	/// <summary>
	/// This class has 2 options to sign in.  Both options results in an access token being sent to the sql connection, the difference
	/// is just how the access token is obtained.  The UserInteractive method will prompt for an azure sign in in a browser window.
	/// The ClientCredentials method will use the client_id and client_secret configured here.  There are other options for the client_credentials
	/// such as using a certificate installed on the machine.  The client credentials are intended for back-end type services or 
	/// Server side auth such as in an ASP.Net web site where the client ( user ) does not have access to the code.
	/// Both cases also set the connection string value for the connection which contains information about the database instance.
	/// </summary>
    static class Azure_SQL
    {
		private const string TENANT_ID = "{your tenant id}";
		private const string CLIENT_ID = "{your client id}"; // Azure_SQL
		private const string CLIENT_SECRET = "{your client secret}";
		private const string REDIRECT_URI = "http://localhost";
		private const string CONNECTION_STRING = "Server=tcp:{your azure database instance name}.database.windows.net,1433;Initial Catalog=RayLab;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False";
		private static readonly string[] scopes = new string[] { "https://database.windows.net/.default" };
		private static readonly string[] scopes2 = new string[] { "User.Read" };

		/// <summary>
		/// Method will prompt user to sign in using azure credentials.  User must be a user in the database as well for the
		/// resulting database call to work.
		/// </summary>
		/// <returns></returns>
		public static async Task<string> GetAccessToken_UserInteractive()
		{

			IPublicClientApplication app = PublicClientApplicationBuilder
				.Create(CLIENT_ID)
				.WithAuthority(AzureCloudInstance.AzurePublic, TENANT_ID)
				.WithRedirectUri(REDIRECT_URI)
				.Build();			

			string accessToken = string.Empty;

			AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
						
            try
            {
				authResult = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
				accessToken = authResult.AccessToken;
            } catch (MsalUiRequiredException)
            {
				authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
				accessToken = authResult.AccessToken;
			} catch (Exception ex)
            {
				Console.WriteLine($"Authentication error: {ex.Message}");
            }

			Console.WriteLine($"Access token: {accessToken}\n");

			return accessToken;
		}

		public static async Task<string> Get_TokenByIntegratedWindows()
        {
			IPublicClientApplication app = PublicClientApplicationBuilder
				.Create("1f29ae22-5b9e-412a-8ed5-3e3a1ce91f53")
				.WithAuthority(AzureCloudInstance.AzurePublic, "72f988bf-86f1-41af-91ab-2d7cd011db47") // microsoft tenant
				.WithRedirectUri(REDIRECT_URI)
				.Build();

			string accessToken = string.Empty;

			AuthenticationResult authResult = null;
			IEnumerable<IAccount> accounts = await app.GetAccountsAsync();

			try
			{
				authResult = await app.AcquireTokenByIntegratedWindowsAuth(scopes2).ExecuteAsync();
				accessToken = authResult.AccessToken;
			}
			catch (MsalUiRequiredException)
			{
				authResult = await app.AcquireTokenInteractive(scopes2).ExecuteAsync();
				accessToken = authResult.AccessToken;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Authentication error: {ex.Message}");
			}

			Console.WriteLine($"Access token: {accessToken}\n");

			return accessToken;
		}

		public static async Task<string> Get_OBOToken(string assertion)
        {
			IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
				.Create(CLIENT_ID)
				.WithClientSecret(CLIENT_SECRET)
				.WithRedirectUri(REDIRECT_URI)
				.Build();

			UserAssertion userAssertion = new UserAssertion(assertion, "urn:ietf:params:oauth:grant-type:jwt-bearer");
			AuthenticationResult result = await app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();

			string accessToken = result.AccessToken;
			return accessToken;

        }

		/// <summary>
		/// Uses client credentials to obtain an access token for the database - the service principal application object name ( not id ) must
		/// be a user in the database ( used a statment like this to add the name of the service principal - my example is Azure_SQL: CREATE USER [Azure_SQL] FROM EXTERNAL PROVIDER)
		/// and have at least the proper role required to perform the action needed ( like db_reader to perform
		/// select statements, for example ). <see cref="https://docs.microsoft.com/en-us/sql/t-sql/statements/alter-role-transact-sql?view=sql-server-ver15#examples"/>
		/// </summary>
		/// <returns></returns>
		public static async Task<string> GetAccessToken_ClientCredentials()
		{

			IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
				.Create(CLIENT_ID)
				.WithClientSecret(CLIENT_SECRET)
				.WithAuthority(AzureCloudInstance.AzurePublic, TENANT_ID)
				.WithRedirectUri(REDIRECT_URI)
				.Build();

			string accessToken = string.Empty;

			AuthenticationResult authResult = null;

			try
			{
				authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
				accessToken = authResult.AccessToken;
			}
			catch (MsalClientException ex)
			{
				Console.Write($"Error obtaining access token: {ex.Message}");
				
			}

			Console.WriteLine($"Access token: {accessToken}\n");

			return accessToken;
		}

		/// <summary>
		/// Pass the access token obtained and uses the connection string to make the connection to the database and execute the request.
		/// </summary>
		/// <param name="access_token"></param>
		/// <returns></returns>
		public static Dictionary<Guid,string> GetUsernames(string access_token)
		{
			using (var connection = new SqlConnection(CONNECTION_STRING))
			{
				connection.AccessToken = access_token;

                try
                {
					connection.Open();

                } catch (Exception ex)
                {
					Console.WriteLine($"DB Connection error: {ex.Message}");
                }
				var cmd = connection.CreateCommand();
				cmd.CommandText = "SELECT TOP 10 id, user_name from Users Order by user_name ASC";
				Console.WriteLine($"Executing command: {cmd.CommandText}...\n");

				var reader = cmd.ExecuteReader();
				var users = new Dictionary<Guid, string>();
				
				if (reader.HasRows)
                {
					while (reader.Read())
					{
						users.Add(reader.GetGuid(reader.GetOrdinal("id")), reader.GetString(reader.GetOrdinal("user_name")));
					}
                }

				return users;
			}
		}
	}
}
