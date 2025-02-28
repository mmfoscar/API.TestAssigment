﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class LinkRepositoryTest
	{
		private Context _context;
		private ILinkRepository _repository;

		[SetUp]
		public void Init()
		{
			_context = new Context();
			_repository = new LinkRepository(_context);
		}

		[TearDown]
		public void Cleanup()
		{
			_context.Dispose();
		}

		[Test]
		public void CreateLink_DomainExist_OneArtistExist()
		{
			var domain = Builder<Domain>.CreateNew()
				.With(d => d.Id, Guid.NewGuid())
				.With(d => d.Name, "domain")
				.Build();
			var artists = Builder<Artist>.CreateListOfSize(2)
				.All()
				.Do(x => x.Id = Guid.NewGuid())
				.Build().ToList();

			// imitates existing entries
			using (var contex = new Context())
			{
				contex.Domains.Add(domain);
				contex.Artists.Add(artists.First());
				contex.SaveChanges();
			}

			var link = Builder<Link>.CreateNew()
				.With(x => x.Id, Guid.NewGuid())
				.With(x => x.Domain, domain)
				.With(x => x.DomainId, domain.Id)
				.With(x => x.Artists, artists)
				.Build();

			var entity = _repository.CreateLink(link);

			var saved = _context.Links.Find(entity.Id);

			Assert.IsTrue(entity.IsActive);

			Assert.IsNotNull(saved);
			Assert.AreEqual(link.Artists.Count, saved.Artists.Count);

		}

		[Test]
		public void CreateLink_DomainExist_ArtistsAreNotExists()
		{
			var domain = Builder<Domain>.CreateNew()
				.With(d => d.Id, Guid.NewGuid())
				.With(d => d.Name, "domain")
				.Build();
			var artists = Builder<Artist>.CreateListOfSize(2)
				.All()
				.Do(x => x.Id = Guid.NewGuid())
				.Build().ToList();

			// imitates existing entries
			using (var contex = new Context())
			{
				contex.Domains.Add(domain);
				contex.SaveChanges();
			}

			var link = Builder<Link>.CreateNew()
				.With(x => x.Id, Guid.NewGuid())
				.With(x => x.Domain, domain)
				.With(x => x.DomainId, domain.Id)
				.With(x => x.Artists, artists)
				.Build();

			var entity = _repository.CreateLink(link);

			var saved = _context.Links.Find(entity.Id);

			Assert.IsTrue(entity.IsActive);

			Assert.IsNotNull(saved);
			Assert.AreEqual(link.Artists.Count, saved.Artists.Count);
		}

        [Test]
        public void UpdateLink_CheckPropertyChanges()
        {
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "domain")
                .Build();

            // imitates existing entries
            using (var contex = new Context())
            {
                contex.Domains.Add(domain);
                contex.SaveChanges();
            }

            var link = Builder<Link>.CreateNew()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Domain, domain)
                .With(x => x.DomainId, domain.Id)
                .Build();

            var entity = _repository.CreateLink(link);

            var linkToUpdate = _context.Entry(link);

            // make some property changes to the created link
            linkToUpdate.Property(x => x.Code).CurrentValue = "code";
            linkToUpdate.Property(x => x.Title).CurrentValue = "title";
            linkToUpdate.Property(x => x.Url).CurrentValue = "url";

            entity = _repository.UpdateLink(link);

            var updated = _context.Links.Find(entity.Id);

            Assert.IsTrue(entity.IsActive);

            Assert.IsNotNull(updated);
            Assert.AreEqual(updated.Code, "code");
            Assert.AreEqual(updated.Title, "title");
            Assert.AreEqual(updated.Url, "url");
        }
    }
}
