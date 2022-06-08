﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using projetoBlog.Models;
using projetoBlog.Models.Home;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace projetoBlog.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var conexao = new AcessoMongoDB();
            var condicao = new BsonDocument();
            var PublicacoesRecentes = await conexao.Publicacoes.Find(condicao).SortByDescending(x => x.DataCriacao).Limit(10).ToListAsync();

            var model = new IndexModel
            {
                PublicacoesRecentes = PublicacoesRecentes
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NovaPublicacao()
        {
            return View(new NovaPublicacaoModel());
        }

        [HttpPost]
        public async Task<ActionResult> NovaPublicacao(NovaPublicacaoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var post = new Publicacao
            {
                Autor = User.Identity.Name,
                Titulo = model.Titulo,
                Conteudo = model.Conteudo,
                Tags = model.Tags.Split(' ', ',', ';'),
                DataCriacao = DateTime.UtcNow,
                Comentarios = new List<Comentario>()
            };

            var conexao = new AcessoMongoDB();
            await conexao.Publicacoes.InsertOneAsync(post);

            return RedirectToAction("Publicacao", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Publicacao(string id)
        {

            var conexao = new AcessoMongoDB();
            var publicacao = await conexao.Publicacoes.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (publicacao == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PublicacaoModel
            {
                Publicacao = publicacao,
                NovoComentario = new NovoComentarioModel
                {
                    PublicacaoId = id
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Publicacoes(string tag = null)
        {
            var conexao = new AcessoMongoDB();
            var posts = new List<Publicacao>();

            if (tag == null)
            {
                var filtro = new BsonDocument();
                posts = await conexao.Publicacoes.Find(filtro).SortByDescending(x => x.DataCriacao).Limit(10).ToListAsync();
            }
            else
            {
                var construtor = Builders<Publicacao>.Filter;
                var condicao = construtor.AnyEq(x => x.Tags, tag);
                posts = await conexao.Publicacoes.Find(condicao).SortByDescending(x => x.DataCriacao).Limit(10).ToListAsync();
            }

            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NovoComentario(NovoComentarioModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Publicacao", new { id = model.PublicacaoId });
            }

            var comentario = new Comentario
            {
                Autor = User.Identity.Name,
                Conteudo = model.Conteudo,
                DataCriacao = DateTime.UtcNow
            };

            var conexao = new AcessoMongoDB();

            var construtor = Builders<Publicacao>.Filter;
            var condicao = construtor.Eq(x => x.Id, model.PublicacaoId);

            var construtorAlteracao = Builders<Publicacao>.Update;
            var condicaoAlteracao = construtorAlteracao.Push(x => x.Comentarios, comentario);

            await conexao.Publicacoes.UpdateOneAsync(condicao, condicaoAlteracao);

            return RedirectToAction("Publicacao", new { id = model.PublicacaoId });
        }
    }
}