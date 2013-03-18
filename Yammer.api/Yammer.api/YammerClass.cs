using System;
using System.Collections.Generic;

namespace Yammer.api
{
    public class SendInvitationResult
    {
        public string status { get; set; }
    }

    public class Impersonate
    {
        public int network_id { get; set; }
        public string token { get; set; }
        public string secret { get; set; }
        public bool modify_messages { get; set; }
        public string authorized_at { get; set; }
        public string created_at { get; set; }
        public bool view_subscriptions { get; set; }
        public int user_id { get; set; }
        public object expires_at { get; set; }
        public bool view_groups { get; set; }
        public bool view_members { get; set; }
        public bool modify_subscriptions { get; set; }
        public bool view_messages { get; set; }
        public string network_permalink { get; set; }
        public string network_name { get; set; }
        public bool view_tags { get; set; }
    }

    #region User
    public class Im
    {
        public string provider { get; set; }
        public string username { get; set; }
    }

    public class PhoneNumber
    {
        public string type { get; set; }
        public string number { get; set; }
    }

    public class EmailAddress
    {
        public string type { get; set; }
        public string address { get; set; }
    }

    public class Contact
    {
        public Im im { get; set; }
        public List<PhoneNumber> phone_numbers { get; set; }
        public bool has_fake_email { get; set; }
        public List<EmailAddress> email_addresses { get; set; }
    }

    public class PreviousCompany
    {
        public object start_year { get; set; }
        public object employer { get; set; }
        public object description { get; set; }
        public object position { get; set; }
        public object end_year { get; set; }
    }

    public class School
    {
        public object school { get; set; }
        public object start_year { get; set; }
        public object degree { get; set; }
        public object description { get; set; }
        public object end_year { get; set; }
    }

    public class Settings
    {
        public string xdr_proxy { get; set; }
    }

    public class UserStats
    {
        public int updates { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
    }

    public class User
    {
        public string job_title { get; set; }
        public int network_id { get; set; }
        public string url { get; set; }
        public bool show_ask_for_photo { get; set; }
        public string web_url { get; set; }
        public string birth_date { get; set; }
        public string type { get; set; }
        public object hire_date { get; set; }
        public Contact contact { get; set; }
        public List<object> schools { get; set; }
        public List<string> external_urls { get; set; }
        public string activated_at { get; set; }
        public List<object> previous_companies { get; set; }
        public string summary { get; set; }
        public string department { get; set; }
        public string timezone { get; set; }
        public string full_name { get; set; }
        public string interests { get; set; }
        public string network_name { get; set; }
        public string significant_other { get; set; }
        public int id { get; set; }
        public List<string> network_domains { get; set; }
        public string kids_names { get; set; }
        public string name { get; set; }
        public string verified_admin { get; set; }
        public string location { get; set; }
        public string expertise { get; set; }
        public string can_broadcast { get; set; }
        public string mugshot_url { get; set; }
        public string mugshot_url_template { get; set; }
        public Settings settings { get; set; }
        public object guid { get; set; }
        public string admin { get; set; }
        public string first_name { get; set; }
        public UserStats stats { get; set; }
        public string state { get; set; }
        public string last_name { get; set; }
    }

    // I don't know why there is 2 different class of user (depend where come from) 
    public class User2
    {
        public Contact contact { get; set; }
        public string department { get; set; }
        public string network_name { get; set; }
        public bool show_ask_for_photo { get; set; }
        public List<string> network_domains { get; set; }
        public List<string> external_urls { get; set; }
        public string timezone { get; set; }
        public Settings settings { get; set; }
        public string name { get; set; }
        public string mugshot_url { get; set; }
        public int network_id { get; set; }
        public string birth_date { get; set; }
        public string admin { get; set; }
        public string type { get; set; }
        public string kids_names { get; set; }
        public string can_broadcast { get; set; }
        public string interests { get; set; }
        public string significant_other { get; set; }
        public string last_name { get; set; }
        public int id { get; set; }
        public string job_title { get; set; }
        public string verified_admin { get; set; }
        public Stats stats { get; set; }
        public string state { get; set; }
        public string expertise { get; set; }
        public string mugshot_url_template { get; set; }
        public List<PreviousCompany> previous_companies { get; set; }
        public object guid { get; set; }
        public string activated_at { get; set; }
        public string full_name { get; set; }
        public List<School> schools { get; set; }
        public string summary { get; set; }
        public string first_name { get; set; }
        public object hire_date { get; set; }
        public string url { get; set; }
        public string web_url { get; set; }
        public string location { get; set; }
    }

    public class ProfileFieldsConfig
    {
        public bool enable_work_phone { get; set; }
        public bool enable_mobile_phone { get; set; }
        public bool enable_job_title { get; set; }
    }

    public class Network
    {
        public string permalink { get; set; }
        public string web_url { get; set; }
        public bool moderated { get; set; }
        public string type { get; set; }
        public bool is_chat_enabled { get; set; }
        public bool show_upgrade_banner { get; set; }
        public bool community { get; set; }
        public string header_text_color { get; set; }
        public bool is_group_enabled { get; set; }
        public string created_at { get; set; }
        public string navigation_background_color { get; set; }
        public int id { get; set; }
        public bool paid { get; set; }
        public string name { get; set; }
        public string header_background_color { get; set; }
        public bool is_org_chart_enabled { get; set; }
        public ProfileFieldsConfig profile_fields_config { get; set; }
        public string navigation_text_color { get; set; }
    }

    public class AccessToken
    {
        public int network_id { get; set; }
        public bool view_messages { get; set; }
        public string token { get; set; }
        public bool view_members { get; set; }
        public object expires_at { get; set; }
        public bool modify_messages { get; set; }
        public string authorized_at { get; set; }
        public bool modify_subscriptions { get; set; }
        public string created_at { get; set; }
        public bool view_groups { get; set; }
        public string network_name { get; set; }
        public string network_permalink { get; set; }
        public bool view_tags { get; set; }
        public int user_id { get; set; }
        public bool view_subscriptions { get; set; }
    }

    public class UserRootObject
    {
        public User user { get; set; }
        public Network network { get; set; }
        public AccessToken access_token { get; set; }
    }
    #endregion

    #region Messages
    public class ThreadedExtended
    {
    }

    public class LikedBy
    {
        public List<object> names { get; set; }
        public int count { get; set; }
    }

    public class Body
    {
        public string plain { get; set; }
        public string rich { get; set; }
        public string parsed { get; set; }
        public List<string> urls { get; set; }
    }

    public class Message
    {
        public string message_type { get; set; }
        public int network_id { get; set; }
        public List<object> attachments { get; set; }
        public LikedBy liked_by { get; set; }
        public string created_at { get; set; }
        public int thread_id { get; set; }
        public bool system_message { get; set; }
        public object chat_client_sequence { get; set; }
        public Body body { get; set; }
        public string client_url { get; set; }
        public string web_url { get; set; }
        public int id { get; set; }
        public int sender_id { get; set; }
        public object replied_to_id { get; set; }
        public string client_type { get; set; }
        public bool direct_message { get; set; }
        public string privacy { get; set; }
        public string url { get; set; }
        public string sender_type { get; set; }
    }

    public class OpenGraphInMessage
    {
        public string og_url { get; set; } // – (required) The canonical URL of the object that will be used as its permanent ID in the graph.
        public string og_title { get; set; } // – The title of your object as it should appear within the graph.
        public string og_image { get; set; } //– A thumbnail image URL which represents your object in the graph.
        public string og_description { get; set; } //– A one to two sentence description of your object.
        public string og_object_type { get; set; } // – The type of your object, e.g., “employee”. Must be a supported type.
        public string og_site_name { get; set; } //– An identifier to relate objects from a common domain, e.g., “Yammer Blog”.
        public string og_meta { get; set; } //– Structured metadata about this object that can be used by clients for custom rendering.
        public bool og_fetch { get; set; } //– Fetch Open Graph attributes over the Internet. (default: false)
    }

    public class Stats
    {
        public string first_reply_at { get; set; }
        public int? first_reply_id { get; set; }
        public string latest_reply_at { get; set; }
        public int latest_reply_id { get; set; }
        public int updates { get; set; }
        public int shares { get; set; }
        public int? followers { get; set; }
        public int? following { get; set; }
    }

    public class Reference
    {
        public string type { get; set; }
        public string web_url { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string url { get; set; }
        public string permalink { get; set; }
        public string normalized_name { get; set; }
        public List<object> topics { get; set; }
        public int? thread_starter_id { get; set; }
        public bool? has_attachments { get; set; }
        public Stats stats { get; set; }
        public bool? direct_message { get; set; }
        public string privacy { get; set; }
        public string mugshot_url { get; set; }
        public string job_title { get; set; }
        public string activated_at { get; set; }
        public string mugshot_url_template { get; set; }
        public string full_name { get; set; }
        public string state { get; set; }
    }

    public class Realtime
    {
        public string authentication_token { get; set; }
        public string channel_id { get; set; }
        public string uri { get; set; }
    }

    public class FollowedReference
    {
        public string type { get; set; }
        public long id { get; set; }
    }

    public class Meta
    {
        public string feed_desc { get; set; }
        public int requested_poll_interval { get; set; }
        public bool older_available { get; set; }
        public List<int> followed_user_ids { get; set; }
        public int current_user_id { get; set; }
        public Realtime realtime { get; set; }
        public bool direct_from_body { get; set; }
        public List<object> ymodules { get; set; }
        public List<FollowedReference> followed_references { get; set; }
        public string feed_name { get; set; }
    }

    public class MessagesRootObject
    {
        public ThreadedExtended threaded_extended { get; set; }
        public List<Message> messages { get; set; }
        public List<Reference> references { get; set; }
        public Meta meta { get; set; }
    }
    #endregion

    #region Activity
    public class Actor
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class Video
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    public class ActivityObject
    {
        public string type { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public Video video { get; set; }
    }

    public class UserForActivity
    {
        public string email { get; set; }
        public string name { get; set; }
    }

    public class Activity
    {
        public Actor actor { get; set; }
        public string action { get; set; }
        public ActivityObject @object { get; set; }
        public Boolean @private { get; set; }
        public string message { get; set; }
        public List<UserForActivity> users { get; set; }
    }

    public class MultipleActivities
    {
        public List<Activity> activity { get; set; }
    }
    #endregion
}
