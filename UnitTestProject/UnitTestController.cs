using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Project.Controllers;
using Project.Data;
using Project.Models;
using Xunit;

public class UnitTestController
{
    private readonly UsersController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<AuthorizationService> _authorizationServiceMock;

    public UnitTestController()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _context = new ApplicationDbContext(options);

        _distributedCacheMock = new Mock<IDistributedCache>();
        _mapperMock = new Mock<IMapper>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _authorizationServiceMock = new Mock<AuthorizationService>(_context);


        _controller = new UsersController(_context, _distributedCacheMock.Object, _mapperMock.Object, _httpClientFactoryMock.Object, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task CreateUser_PasswordTooShort()
    {
        var userDto = new UserDto { FirstName = "John", LastName = "Doe", Age = 30 };
        var password = "12345";
        var result = await _controller.CreateUser(userDto, password);
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
