using AutoFixture;
using Catalog.API;
using Catalog.API.Controllers;
using Catalog.API.Models;
using Catalog.Domain;
using Catalog.UnitTests.TestFixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Catalog.UnitTests
{
    public class PlatesControllerTests
    { 
        private readonly IFixture _fixture = new Fixture();
        private PlatesController SUT { get; }
        private readonly DatabaseTestFixture _databaseTestFixture = new ();

        public PlatesControllerTests() {
            SUT = new PlatesController(_databaseTestFixture.Database);
        }

        [Fact]
        public async Task CanReturnPaginatedListOfPlates()
        {
            var plates = _fixture.CreateMany<Plate>(50);
            _databaseTestFixture.Database.Plates.AddRange(plates);
            _databaseTestFixture.Database.SaveChanges();

            var getPlatesResponse = await SUT.Index(pageNumber:1);
            getPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>()
                .Which
                .Items
                .Should().HaveCount(20);           
        }

        [Fact]
        public async Task ListOfPlatesIncludes20PercentMarkup()
        {
            var plates = _fixture.CreateMany<Plate>(50);
            _databaseTestFixture.Database.Plates.AddRange(plates);
            _databaseTestFixture.Database.SaveChanges();

            var getPlatesResponse = await SUT.Index(pageNumber: 1);
            getPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>()
                .Which.Items
                .Should().AllSatisfy(p => p.SalePriceWithMarkup.Should().Be(p.SalePrice * 1.2m));
        }

        [Fact]
        public async Task CanAddPlate()
        {
            var plates = _fixture.CreateMany<Plate>(50);
            _databaseTestFixture.Database.Plates.AddRange(plates);
            _databaseTestFixture.Database.SaveChanges();

            var plate = _fixture.Create<Plate>();

            var addPlateResponse = await SUT.Add(plate);
            addPlateResponse.Should().BeOfType<CreatedResult>()
                .Which.Value.Should().BeEquivalentTo(plate);

            var getPlatesResponse = await SUT.Index(pageNumber:1, pageSize:60);
            getPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>()
                .Which.Items
                .Should().Contain(p => p.Registration == plate.Registration);
        }

        [Fact]
        public async Task CanPaginatePlates()
        {
            var plates = _fixture.CreateMany<Plate>(50);
            _databaseTestFixture.Database.Plates.AddRange(plates);
            _databaseTestFixture.Database.SaveChanges();

            var getFirstPageOfPlatesResponse = await SUT.Index(pageNumber: 1, pageSize: 10);
            var getSecondPageOfPlatesResponse = await SUT.Index(pageNumber: 2, pageSize: 10);
            var firstPage = getFirstPageOfPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>().Subject;
            
            var secondPage = getSecondPageOfPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>().Subject;

            firstPage.TotalItems.Should().Be(secondPage.TotalItems).And.Be(50);            
            firstPage.PageSize.Should().Be(secondPage.PageSize).And.Be(10);
            firstPage.TotalPages.Should().Be(secondPage.TotalPages).And.Be(5);
            firstPage.PageNumber.Should().Be(1);
            secondPage.PageNumber.Should().Be(2);
            secondPage.Items.Should().NotContainEquivalentOf(firstPage.Items);
        }

        [Fact]
        public async Task CanSortPlatesByPrice()
        {
            var plates = _fixture.CreateMany<Plate>(10);
            _databaseTestFixture.Database.Plates.AddRange(plates);
            _databaseTestFixture.Database.SaveChanges();

            var getPlatesResponse = await SUT.Index(
                pageNumber: 1,
                pageSize: 10,
                sortBy: nameof(Plate.SalePrice),
                sortOrder: "desc");

            var sortedPlates = getPlatesResponse
                .Should().BeOfType<OkObjectResult>()
                .Which.Value
                .Should().BeOfType<PaginatedList<Plate>>().Subject;

            sortedPlates.Items.Should().BeInDescendingOrder(p => p.SalePrice);

        }
    }
}