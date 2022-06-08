using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace projetoBlog.Models
{
    public class AcessoMongoDB
    {
        public const string CONNECTION_STRING = "mongodb://127.0.0.1:27017";
        public const string DATABASE = "Blog";
        public const string PUBLISHIES_COLLECTION = "Publicacoes";
        public const string USERS_COLLECTION = "Usuarios";


        private static readonly IMongoClient _client;
        private static readonly IMongoDatabase _database;

        static AcessoMongoDB()
        {
            _client = new MongoClient(CONNECTION_STRING);
            _database = _client.GetDatabase(DATABASE);
        }

        public IMongoClient Client
        {
            get { return _client; }
        }

        public IMongoCollection<Usuario> Usuarios
        {
            get { return _database.GetCollection<Usuario>(USERS_COLLECTION); }
        }

        public IMongoCollection<Publicacao> Publicacoes
        {
            get { return _database.GetCollection<Publicacao>(PUBLISHIES_COLLECTION); }
        }

    }
}
