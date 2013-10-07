using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using RestSharp.Deserializers;

namespace Yammer.api
{
    public class YammerClient
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
        /// Defines URI of  preauthorized oauth access token for a given user_id/consumer_key combination.
        /// </summary>
        private const String ImpersonateTokenService = "/api/v1/oauth.json";
        /// <summary> 
        /// Defines URI of service which redirect to login/autorized
        /// </summary>
        private const String AccessCodeService = "/dialog/oauth";
        /// <summary>
        /// Defines URI of service which sends invitation to user
        /// </summary>
        private const String InvitationsService = "/api/v1/invitations.json";
        /// <summary>
        /// Defines URI of service which get user info/details by mail.
        /// </summary>
        private const String UserServiceByMail = "api/v1/users/by_email.json";
        /// <summary>
        /// Defines URI of service which get users list.
        /// </summary>
        private const String UsersService = "api/v1/users.json";
        /// <summary>
        /// Defines URI of service which get users list.
        /// </summary>
        private const String CurrentUserService = "api/v1/users/current.json";
        /// <summary>
        /// Defines URI of service which post a message
        /// </summary>
        private const String PostMessageService = "api/v1/messages.json";
        /// <summary>
        /// Standard configuration container
        /// </summary>
        private readonly ClientConfigurationContainer _configuration;
        /// <summary>
        /// Contain current user info when we obtain a token
        /// </summary>
        private UserRootObject _userRootObject;
        /// <summary>
        /// Persistant Rest client for Yammer
        /// </summary>
        private RestClient _yammerRestClient;
        /// <summary>
        /// Accessor to a singleton request (restsharp) object
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
        /// A cached list of known users from Yammer
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
        /// Access token returned by provider. Can be used for further calls of provider API.
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
        /// Set a new Authorisation (from a user)
        /// And try to get a new token (for this user) 
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
        /// Returns URI of service which should be called in order to start authentication process.
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


            var uri=this.YammerRestClient.BuildUri(request).ToString();
            return String.Format(uri, _configuration.RedirectUri);
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        /// <returns>Yammer User info</returns>
        public User GetUserInfo()
        {
            if (String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }
            return this._userRootObject != null ? this._userRootObject.user : null;
        }

        /// <summary>
        /// Get an app token
        /// </summary>
        /// <returns>the token</returns>
        public String GetToken()
        {
            if (String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }
            return this.AccessToken;
        }

        /// <summary>
        /// Check if the user exist in Yammer
        /// </summary>
        /// <param name="mail">Mail address of the user</param>
        /// <returns>True = he is in Yammer</returns>
        public Boolean ExistUser(String mail)
        {
            return this.GetUserByMail(mail) != null;
        }

        /// <summary>
        /// iInd a user in the cached list of users
        /// </summary>
        /// <param name="mail">The mail to find</param>
        /// <returns>User info (or null if not found)</returns>
        public User FindUserFromYammerList(String mail)
        {
            return mail == null ? null : this.GetUsers().FirstOrDefault(user => user.contact.email_addresses.Exists(p => p.address == mail));
        }

        /// <summary>
        /// Find a user by his mail
        /// </summary>
        /// <param name="mail">The mail of user</param>
        /// <returns>User (2nd type of object) info (or null if not found)</returns>
        public User2 GetUserByMail(String mail)
        {
            return String.IsNullOrWhiteSpace(mail) ? null : (this.YammerRequest<User2>(UserServiceByMail, Method.GET, new { email = mail }));
        }

        /// <summary>
        /// Try to obtain a user's token
        /// - try different methods to obtain it
        /// </summary>
        /// <param name="mail">The user mail</param>
        /// <param name="obtainToken">force to ask an explicit impersonation</param>
        /// <returns>The token (or null)</returns>
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
                        // try to obtain a impersonate token (work only for verified admin)
                        sRet = this.AskImpersonateToken(usr.id);
                    }
                }
            }
            return sRet;
        }

        private String AskImpersonateToken(int userId)
        {
            // todo : must be finished when I have sufficient right
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
        /// Send an invitation ti a new user
        /// </summary>
        /// <param name="mailAdress">EMaik address of the new user</param>
        /// <returns>Status ok if it's done</returns>
        public SendInvitationResult SendInvitation(String mailAdress)
        {
            return (this.YammerRequest<SendInvitationResult>(InvitationsService, Method.POST, new { email = mailAdress }));
        }

        /// <summary>
        /// Send an invitation ti a new user
        /// </summary>
        /// <returns>The current user (the owner of token)</returns>
        public User GetCurrentUser()
        {
            return (this.YammerRequest<User>(CurrentUserService));
        }

        /// <summary>
        /// Post a simple message
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topic">topic of the message</param>
        /// <returns>a completed message (with the id)</returns>
        public MessagesRootObject PostMessage(String messageToPost, long groupId, String topic)
        {
            return this.PostAnyMessage(new { body = messageToPost, group_id = groupId, topic1 = topic });
        }

        /// <summary>
        /// Post a message with an open graph object
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topic">topic of the message</param>
        /// <param name="og">OpenGraph object</param>
        /// <returns></returns>
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
        /// Post a message with an open graph object
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topics">List of topics</param>
        /// <param name="og">OpenGraph object</param>
        /// <returns></returns>
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
        /// Get all users list
        /// </summary>
        /// <returns>A list of Yammer users</returns>
        public List<User> GetUsers()
        {
            return (this.YammerUsers);
        }

        /// <summary>
        /// Get all users impersonante token
        /// </summary>
        /// <returns>a list of yammer impersonante info</returns>
        public List<Impersonate> GetImpersonateTokens()
        {
            return (this.YammerRequest<List<Impersonate>>(ImpersonateUsersService));
        }

        // todo : implemente impersonation
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
        /// Query for access token and parses response.
        /// </summary>
        public void GetAccessToken()
        {
            this._userRootObject = this.YammerRequest<UserRootObject>(AccessTokenService, Method.GET, new
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
        /// Query Yammer and parse JSON response in define object
        /// </summary>
        /// <typeparam name="T">Object type Expected</typeparam>
        /// <param name="restService">Service uri to query</param>
        /// <param name="method">Get or Post</param>
        /// <param name="objectForRequest">Other parameters embedded in an object</param>
        /// <param name="getAuth">Try to get token</param>
        /// <returns>The JSON response parse in T type</returns>
        private T YammerRequest<T>(String restService, Method method = Method.GET, Object objectForRequest = null, Boolean getAuth = true)
            where T : class
        {
            if (getAuth && String.IsNullOrWhiteSpace(this.AccessToken))
            {
                this.GetAccessToken();
            }

            var request = new RestRequest { Resource = restService, Method = method };
            request.AddHeader("Authorization", "Bearer " + this.AccessToken);
            if (objectForRequest != null)
            {
                request.AddObject(objectForRequest);
            }

            var response = this.YammerRestClient.Execute(request);

            // response sent in JSON format and deserialized
            try
            {
                var  deserializer=new JsonDeserializer();

                var ret = deserializer.Deserialize<T>(response);//  JsonConvert.DeserializeObject<T>(response.Content);
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Post any type of message (eg simple vs with opengraph)
        /// </summary>
        /// <param name="obj">Message container</param>
        /// <returns>A message container completed</returns>
        private MessagesRootObject PostAnyMessage(object obj)
        {
            return (this.YammerRequest<MessagesRootObject>(PostMessageService, Method.POST, obj));
        }

    }

}
