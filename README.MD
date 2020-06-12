Place a Config file in the solution containing settings

````
    public struct Config
    {
        // database
        public const string ConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=mtg_metagamer;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        // MCM client
        public const string AppToken = "";
        public const string AppSecret = "";
        public const string AccessToken = "";
        public const string AccessSecret = "";
    }
````
