using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;

namespace Yammer.api
{
    public partial class YammerClient
    {
        /// <summary>
        /// Query Yammer and parse JSON response in define object
        /// </summary>
        /// <typeparam name="T">Object type Expected</typeparam>
        /// <param name="restService">Service uri to query</param>
        /// <param name="method">Get or Post</param>
        /// <param name="objectForRequest">Other parameters embedded in an object</param>
        /// <param name="getAuth">Try to get token</param>
        /// <returns>The JSON response parse in T type</returns>
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
                //Request Format set to JSON and AddBody instead of AddObject 
                //are necessary to allow posting complex objects (such as the Activity object)
                request.RequestFormat = DataFormat.Json;
                request.AddBody(objectForRequest);
            }
            var tcs = new TaskCompletionSource<T>();

            this.YammerRestClient.ExecuteAsync(request, reponseasync =>
              {
                  // response sent in JSON format and deserialized
                  try
                  {
                      var deserializer = new JsonDeserializer();
                      var ret = deserializer.Deserialize<T>(reponseasync);
                      tcs.SetResult(ret);
                  }
                  catch (Exception ex)
                  {
                      Trace.TraceError("Exception in YammerRequestAsync: {0} | Source:{1} | StackTrace:{2}", ex.Message, ex.Source, ex.StackTrace);
                      throw;
                  }
              });
            return tcs.Task;
        }

        /// <summary>
        /// Post any type of message (eg simple vs with opengraph)
        /// </summary>
        /// <param name="obj">Message container</param>
        /// <returns>A message container completed</returns>
        private Task<MessagesRootObject> PostAnyMessageAsync(object obj)
        {
            return (this.YammerRequestAsync<MessagesRootObject>(PostMessageService, Method.POST, obj));
        }

        /// <summary>
        /// Post a simple message
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topic">topic of the message</param>
        /// <returns>a completed message (with the id)</returns>
        public Task<MessagesRootObject> PostMessageAsync(String messageToPost, long groupId, String topic)
        {
            return this.PostAnyMessageAsync(new { body = messageToPost, group_id = groupId, topic1 = topic });
        }

        /// <summary>
        /// Post a message with an open graph object
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topic">topic of the message</param>
        /// <param name="og">OpenGraph object</param>
        /// <returns></returns>
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
        /// Post a message with an open graph object
        /// </summary>
        /// <param name="messageToPost">Body</param>
        /// <param name="groupId">The group where I post the message</param>
        /// <param name="topics">List of topics</param>
        /// <param name="og">OpenGraph object</param>
        /// <returns></returns>
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

    }
}
