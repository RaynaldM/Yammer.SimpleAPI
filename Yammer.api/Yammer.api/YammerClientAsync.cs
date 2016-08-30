using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;


namespace Yammer.api
{
    public partial class YammerClient
    {
        /// <summary>
        /// Query Yammer and parse JSON response to define the object.
        /// </summary>
        /// <typeparam name="T">Object type Expected</typeparam>
        /// <param name="restService">Service URI to query</param>
        /// <param name="method">GET or POST</param>
        /// <param name="objectForRequest">Other parameters embedded in an object</param>
        /// <param name="getAuth">Try to get the access token</param>
        /// <returns>The JSON response parse of type T</returns>
        public Task<T> YammerRequestAsync<T>(String restService, Method method = Method.GET, Object objectForRequest = null, Boolean getAuth = true)
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
                request.AddBody(objectForRequest);
            }
            var tcs = new TaskCompletionSource<T>();

            this.YammerRestClient.ExecuteAsync(request, responseasync =>
              {
                  // response sent in JSON format and deserialized
                  try
                  {
                      var deserializer = new JsonDeserializer();
                      var ret = deserializer.Deserialize<T>(responseasync);
                      tcs.SetResult(ret);
                  }
                  catch (Exception ex)
                  {
                      Trace.TraceError("Yammer Response:{0} | Exception:{1} | Source:{2} | StackTrace:{3}", responseasync.Content, ex.Message, ex.Source, ex.StackTrace);
                      throw;
                  }
              });
            return tcs.Task;
        }

        /// <summary>
        /// Post any type of message.
        /// </summary>
        /// <param name="obj">Message container</param>
        /// <returns>The threaded task's message object (with its returned ID)</returns>
        private Task<MessagesRootObject> PostAnyMessageAsync(object obj)
        {
            return (this.YammerRequestAsync<MessagesRootObject>(PostMessageService, Method.POST, obj));
        }

        /// <summary>
        /// Post a simple group message.
        /// </summary>
        /// <param name="messageToPost">The message body</param>
        /// <param name="groupId">The group where the message will appear</param>
        /// <param name="topic">The message topic</param>
        /// <returns>The threaded task's message object (with its returned ID)</returns>
        public Task<MessagesRootObject> PostMessageAsync(String messageToPost, long groupId, String topic)
        {
            return this.PostAnyMessageAsync(new { body = messageToPost, group_id = groupId, topic1 = topic });
        }

        /// <summary>
        /// Post a message with an open graph object
        /// </summary>
        /// <param name="messageToPost">The message body</param>
        /// <param name="groupId">The group where the message will appear</param>
        /// <param name="topic">The message topic</param>
        /// <param name="og">The OpenGraph object</param>
        /// <returns>The threaded task's message object (with its returned ID)</returns>
        public Task<MessagesRootObject> PostMessageAsync(String messageToPost, long groupId, String topic, OpenGraphInMessage og)
        {
            return this.PostAnyMessageAsync(new
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
        /// <returns>The threaded task's message object (with its returned ID)</returns>
        public Task<MessagesRootObject> PostMessageAsync(String messageToPost, long groupId, List<String> topics, OpenGraphInMessage og)
        {
            return this.PostAnyMessageAsync(new
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
        /// <returns>The threaded task's message object (with its returned ID)</returns>
        public Task<MessagesRootObject> PostInstantMessageAsync(string messageToPost, long userId, string topic = null)
        {
            return this.PostAnyMessageAsync(new { body = messageToPost, direct_to_id = userId, topic1 = topic });
        }

        /// <summary>
        /// Retrieve all private message objects, based on the DateTime argument.
        /// </summary>
        /// <param name="newerThan">The cutoff timestamp (UTC formatted)</param>
        /// <returns>The threaded task's list of message objects</returns>
        public Task<List<Message>> RetrieveInstantMessagesAsync(DateTime newerThan)
        {
            var tcs = new TaskCompletionSource<List<Message>>();
            var messages = this.YammerRequest<MessagesRootObject>(PrivateMessageService, Method.GET).messages;
            List<Message> newerMessages = (from m in messages
                                           where DateTime.Parse(m.created_at).ToUniversalTime() >= newerThan.ToUniversalTime()
                                           select m).ToList();
            tcs.SetResult(newerMessages);
            return tcs.Task;
        }

    }
}
