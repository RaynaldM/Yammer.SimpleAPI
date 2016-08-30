using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RestSharp;
using RestSharp.Deserializers;

namespace Yammer.api
{
    public partial class YammerClient
    {
        private const String YammerBaseUrl = "https://www.yammer.com";
        /// <summary>
        /// Defines URI of service which issues access token.
        /// </summary>
        private const String AccessTokenService = "/oauth2/access_token";
        /// <summary>
        /// Defines URI of service which gets impersonates info/tokens.
        /// </summary>
        private const String ImpersonateUsersService = "/api/v1/oauth/tokens.json";
        /// <summary>
        /// Defines URI of preauthorized oauth access token for a given user_id/consumer_key combination.
        /// </summary>
        private const String ImpersonateTokenService = "/api/v1/oauth.json";
        /// <summary> 
        /// Defines URI of service which redirects to login/authorized.
        /// </summary>
        private const String AccessCodeService = "/dialog/oauth";
        /// <summary>
        /// Defines URI of service which sends invitation to user.
        /// </summary>
        private const String InvitationsService = "/api/v1/invitations.json";
        /// <summary>
        /// Defines URI of service which gets user info/details by mail.
        /// </summary>
        private const String UserServiceByMail = "api/v1/users/by_email.json";
        /// <summary>
        /// Defines URI of service which gets users list.
        /// </summary>
        private const String UsersService = "api/v1/users.json";
        /// <summary>
        /// Defines URI of service which gets currently authenticated user.
        /// </summary>
        private const String CurrentUserService = "api/v1/users/current.json";
        /// <summary>
        /// Defines URI of service which posts and gets messages.
        /// </summary>
        private const String PostMessageService = "api/v1/messages.json";
        /// <summary>
        /// Defines URI of service which handles private (instant) messages.
        /// </summary>
        private const String PrivateMessageService = "api/v1/messages/private.json";
        /// <summary>
        /// Standard configuration container.
        /// </summary>
        private readonly ClientConfigurationContainer _configuration;
        /// <summary>
        /// Contain current user info when we obtain a token.
        /// </summary>
        private UserRootObject _userRootObject;
        /// <summary>
        /// Persistent REST client for Yammer.
        /// </summary>
        private RestClient _yammerRestClient;
        /// <summary>
        /// Accessor to a singleton request (RestSharp) object.
        /// </summary>
        protected RestClient YammerRestClient
        {
            get { return this._yammerRestClient ?? (this._yammerRestClient = new RestClient(YammerBaseUrl)); }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Yammer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public YammerClient(ClientConfigurationContainer configuration)
        {
            this._configuration = configuration;
        }

        private List<User> _cachedYammerUsers;
        /// <summary>
        /// A cached list of known Yammer users.
        /// </summary>
        public List<User> YammerUsers
        {
            get
            {
                if (_cachedYammerUsers == null)
                    this._cachedYammerUsers = this.YammerRequest<List<User>>(UsersService);
                return this._cachedYammerUsers;
            }
        }

        /// <summary>
        /// Access token returned by the provider. Can be used for further calls to provider API.
        /// </summary>
        public string AccessToken { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Yammer"/> class.
        /// </summary>
        /// <param name="token">A token use to do action</param>
        public YammerClient(String token)
        {
            this.AccessToken = token;
        }

        /// <summary>
        /// Set a new Authorization (from a user)
        /// And try to get a new token (for this user). 
        /// </summary>
        /// <param name="code"></param>
        public void SetAuthorizationCode(String code)
        {
            if (this._configuration.ClientCode != code)
            {
                this._configuration.ClientCode = code;
                this.GetAccessToken();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Yammer"/> class.
        /// </summary>
        /// <param name="clientId">Application client id</param>
        /// <param name="secretKey">Secret Application key</param>
        /// <param name="redirect">Url after Yammer authorize screen</param>
        /// <param name="code">Authorized code from yammer</param>
        /// <param name="token">token from previous session</param>
        public YammerClient(String clientId, String secretKey, String redirect = null, String code = null,
                            string token = null)
            : this(
                new ClientConfigurationContainer
                {
                    ClientId = clientId,
                    ClientSecret = secretKey,
                    ClientCode = code,
                    RedirectUri = redirect
                })
        {
            this.AccessToken = token;
        }

        /// <summary>
        /// Returns URI of service which should be called in order to start the authentication process.
        /// This URI should be used for rendering login link.
        /// </summary>
        /// <remarks>
        /// Any additional information that will be posted back by service.
        /// </remarks>
        public string GetLoginLinkUri(string state = null)
        {
            var request = new RestRequest { Resource = AccessCodeService };

            request.AddObject(new
            {
                client_id = _configuration.ClientId,
                redirect_uri = "{0}",//_configuration.RedirectUri,
                state
            });


            var uri = this.YammerRestClient.BuildUri(request).ToString();
            return String.Format(uri, _configuration.RedirectUri);
        }

        /// <summary>
        /// Get current user info.
        /// </summary>
        /// <returns>Yammer User object</returns>
        public User GetUserInfo()
        {
            if (String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }
            return this._userRootObject != null ? this._userRootObject.user : null;
        }

        /// <summary>
        /// Get an access token.
        /// </summary>
        /// <returns>The access token</returns>
        public String GetToken()
        {
            if (String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }
            return this.AccessToken;
        }

        /// <summary>
        /// Check if the user exists in Yammer.
        /// </summary>
        /// <param name="mail">The email address of the user</param>
        /// <returns>True = Is in Yammer</returns>
        public Boolean ExistUser(String mail)
        {
            return this.GetUserByMail(mail) != null;
        }

        /// <summary>
        /// Find a user in the cached user list.
        /// </summary>
        /// <param name="mail">The email address to find</param>
        /// <returns>The User object (or NULL if not found)</returns>
        public User FindUserFromYammerList(String mail)
        {
            return mail == null ? null : this.GetUsers().FirstOrDefault(user => user.contact.email_addresses.Exists(p => p.address == mail));
        }

        /// <summary>
        /// Find a user by his email.
        /// </summary>
        /// <param name="mail">The email address to find</param>
        /// <returns>The User2 object (or NULL if not found)</returns>
        public User2 GetUserByMail(String mail)
        {
            return String.IsNullOrWhiteSpace(mail) ? null : (this.YammerRequest<User2>(UserServiceByMail, Method.GET, new { email = mail }));
        }

        /// <summary>
        /// Try to obtain a user's token
        /// - try different methods to obtain this.
        /// </summary>
        /// <param name="mail">The email address to find</param>
        /// <param name="obtainToken">Force to ask an explicit impersonation</param>
        /// <returns>The access token (or NULL if not found)</returns>
        public String GetTokenFromUser(String mail, Boolean obtainToken = false)
        {
            String sRet = null;
            var usr = this.GetUserByMail(mail);
            if (usr != null)
            {
                var tokens = this.GetImpersonateTokens();
                if (tokens != null && tokens.Any())
                {
                    var impersonate = tokens.FirstOrDefault(p => p.user_id == usr.id);
                    if (impersonate != null)
                        sRet = impersonate.token;
                    else if (obtainToken)
                    {
                        // Try to obtain an impersonation token (works only for a verified admin).
                        sRet = this.AskImpersonateToken(usr.id);
                    }
                }
            }
            return sRet;
        }

        /// <summary>
        /// Request an access token to impersonate the specified user.
        /// </summary>
        /// <param name="userId">The User object ID</param>
        /// <returns>The access token string</returns>
        private String AskImpersonateToken(int userId)
        {
            // TO-DO : Must be finished when we have sufficient rights.
            var request = new RestRequest { Resource = ImpersonateTokenService, Method = Method.POST };
            request.AddHeader("Authorization", "Bearer " + this.AccessToken);
            var objectForRequest = new { user_id = userId, consumer_key = this._configuration.ClientId };
            request.AddObject(objectForRequest);

            var response = this.YammerRestClient.Execute(request);
            if (response.Content == null || response.Content.Contains("fail"))
                return null;

            return response.Content;
        }

        /// <summary>
        /// Send an invitation to a new user.
        /// </summary>
        /// <param name="mailAdress">Email address of the new user</param>
        /// <returns>Status == OK if it's completed</returns>
        public SendInvitationResult SendInvitation(String mailAdress)
        {
            return (this.YammerRequest<SendInvitationResult>(InvitationsService, Method.POST, new { email = mailAdress }));
        }

        /// <summary>
        /// Obtain the User object for the currently authenticated user.
        /// </summary>
        /// <returns>The current user (the owner of token)</returns>
        public User GetCurrentUser()
        {
            return (this.YammerRequest<User>(CurrentUserService));
        }

        /// <summary>
        /// Post a simple group message.
        /// </summary>
        /// <param name="messageToPost">The message body</param>
        /// <param name="groupId">The group where the message will appear</param>
        /// <param name="topic">The message topic</param>
        /// <returns>The message object (with its returned ID)</returns>
        public MessagesRootObject PostMessage(String messageToPost, long groupId, String topic)
        {
            return this.PostAnyMessage(new { body = messageToPost, group_id = groupId, topic1 = topic });
        }

        /// <summary>
        /// Post a message with an OpenGraph object.
        /// </summary>
        /// <param name="messageToPost">The message body</param>
        /// <param name="groupId">The group where the message will appear</param>
        /// <param name="topic">The message topic</param>
        /// <param name="og">The OpenGraph object</param>
        /// <returns>The message object (with its returned ID)</returns>
        public MessagesRootObject PostMessage(String messageToPost, long groupId, String topic, OpenGraphInMessage og)
        {
            return this.PostAnyMessage(new
            {
                body = messageToPost,
                group_id = groupId,
                topic1 = topic,
                og.og_url,
                og.og_title,
                og.og_image,
                og.og_description,
                og.og_object_type,
                og.og_site_name,
                og.og_meta,
                og.og_fetch
            });
        }

        /// <summary>
        /// Post a message with an OpenGraph object and multiple topics.
        /// </summary>
        /// <param name="messageToPost">The messsage body</param>
        /// <param name="groupId">The group where the message will appear</param>
        /// <param name="topics">The list of topics</param>
        /// <param name="og">The OpenGraph object</param>
        /// <returns>The message object (with its returned ID)</returns>
        public MessagesRootObject PostMessage(String messageToPost, long groupId, List<String> topics, OpenGraphInMessage og)
        {
            return this.PostAnyMessage(new
            {
                body = messageToPost,
                group_id = groupId,
                topic1 = topics[0],
                topic2 = topics[1],
                topic3 = topics[2],
                og.og_url,
                og.og_title,
                og.og_image,
                og.og_description,
                og.og_object_type,
                og.og_site_name,
                og.og_meta,
                og.og_fetch
            });
        }

        /// <summary>
        /// Post a private message to specific user.
        /// </summary>
        /// <param name="messageToPost">The message body</param>
        /// <param name="userId">The receipient's ID</param>
        /// <param name="topic">The message topic (NULL for the private message type)</param>
        /// <returns>The message object (with its returned ID)</returns>
        public MessagesRootObject PostInstantMessage(string messageToPost, long userId, string topic = null)
        {
            return this.PostAnyMessage(new { body = messageToPost, direct_to_id = userId, topic1 = topic });
        }

        /// <summary>
        /// Retrieve all private message objects, based on the DateTime argument.
        /// </summary>
        /// <param name="newerThan">The cutoff timestamp (UTC formatted)</param>
        /// <returns>A list of message objects</returns>
        public List<Message> RetrieveInstantMessages(DateTime newerThan)
        {
            var messages = this.YammerRequest<MessagesRootObject>(PrivateMessageService, Method.GET).messages;
            List<Message> newerMessages = (from m in messages
                                           where DateTime.Parse(m.created_at).ToUniversalTime() >= newerThan.ToUniversalTime()
                                           select m).ToList();
            return newerMessages;
        }

        /// <summary>
        /// Get a list of all users.
        /// </summary>
        /// <returns>A list of User objects</returns>
        public List<User> GetUsers()
        {
            return (this.YammerUsers);
        }

        /// <summary>
        /// Get a list of all user's impersonation tokens.
        /// </summary>
        /// <returns>A list of Impersonante objects</returns>
        public List<Impersonate> GetImpersonateTokens()
        {
            return (this.YammerRequest<List<Impersonate>>(ImpersonateUsersService));
        }

        // TO-DO : Implement impersonation.
        [Obsolete("Don't use this method, it should be refactored")]
        public String PostActivity(Actor actor, String action, ActivityObject activityObject, String message,
            Boolean @private, List<UserForActivity> users)
        {
            return (this.YammerRequest<String>(InvitationsService, Method.POST,
                                               new
                                               {
                                                   activity = new Activity
                                                   {
                                                       actor = actor,
                                                       action = action,
                                                       @object = activityObject,
                                                       message = message,
                                                       @private = @private,
                                                       users = users
                                                   }
                                               }));
        }

        /// <summary>
        /// Query for access token and parse the response.
        /// </summary>
        public void GetAccessToken()
        {
            this._userRootObject = this.YammerRequest<UserRootObject>(AccessTokenService, Method.POST, new
            {
                code =
                                                                                             _configuration.ClientCode,
                client_id =
                                                                                             _configuration.ClientId,
                client_secret =
                                                                                             _configuration.ClientSecret,
                redirect_uri =
                                                                                             _configuration.RedirectUri,
                grant_type =
                                                                                             "authorization_code"
            }, false);


            this.AccessToken = this._userRootObject != null ? this._userRootObject.access_token.token : null;
        }

        /// <summary>
        /// Query Yammer and parse JSON response to define the object.
        /// </summary>
        /// <typeparam name="T">Object type expected</typeparam>
        /// <param name="restService">Service URI to query</param>
        /// <param name="method">GET or POST</param>
        /// <param name="objectForRequest">Other parameters embedded in an object</param>
        /// <param name="getAuth">Try to get the access token</param>
        /// <param name="useBody">Indicate to use AddBody (true : default) or AddObject (false)</param>
        /// <returns>The JSON response parse of type T</returns>
        public T YammerRequest<T>(String restService, Method method = Method.GET, Object objectForRequest = null,
            Boolean getAuth = true, Boolean useBody = true)
            where T : class
        {
            if (getAuth && String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }

            var request = new RestRequest { Resource = restService, Method = method };
            if (this.AccessToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + this.AccessToken);
            }
            if (objectForRequest != null)
            {
                // Request format set to JSON and AddBody instead of AddObject 
                // is necessary to allow posting complex objects (such as the Activity object).
                request.RequestFormat = DataFormat.Json;
                if (useBody)
                {
                    request.AddBody(objectForRequest);
                }
                else
                {
                    request.AddObject(objectForRequest);
                }
            }

            var response = this.YammerRestClient.Execute(request);

            // Response sent in JSON format and deserialized.
            try
            {
                var deserializer = new JsonDeserializer();
                var ret = deserializer.Deserialize<T>(response);    // JsonConvert.DeserializeObject<T>(response.Content);
                return ret;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Yammer Response:{0} | Exception:{1} | Source:{2} | StackTrace:{3}", response.Content, ex.Message, ex.Source, ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Post any type of message.
        /// </summary>
        /// <param name="obj">Message container</param>
        /// <returns>The message object (with its returned ID)</returns>
        private MessagesRootObject PostAnyMessage(object obj)
        {
            return (this.YammerRequest<MessagesRootObject>(PostMessageService, Method.POST, obj));
        }

    }

}
