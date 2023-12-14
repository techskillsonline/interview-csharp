using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HashidsNet;
using Moq;
using Moq.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Url.Commands;
using UrlShortenerService.Domain.Entities;

namespace UnitTesting.Application.Tests;
public class CreateShortUrlCommandTest
{
    private readonly Mock<IApplicationDbContext> _mockdb = default!;
    private readonly Mock<IHashids> _mockhashids = default!;
    private readonly Mock<IHashGeneratorService> _mockhashsvc = default!;

    public CreateShortUrlCommandTest()
    {
        _mockdb = new Mock<IApplicationDbContext>();
        _mockhashids = new Mock<IHashids>();
        _mockhashsvc = new Mock<IHashGeneratorService>();
    }
    [Fact]
    public void CreateShortUrlCommand_Handle_CompactURLAlreadyExists()
    {
        //Arrange
        CreateShortUrlCommand request = new CreateShortUrlCommand
        {
            Url = "https://www.google.com"
        };
        Url url1 = new Url { OriginalUrl = "https://www.google.com", CompactUrl = "abc123" };
        Url url2 = new Url { OriginalUrl = "https://www.amazon.com", CompactUrl = "abc124" };

        Url[] urls = new Url[] { url1, url2 };
        _ = _mockdb.Setup(i => i.Urls).ReturnsDbSet(urls);

        //Act
        CreateShortUrlCommandHandler hndlr = new CreateShortUrlCommandHandler(_mockdb.Object, _mockhashids.Object, _mockhashsvc.Object);
        var obj = hndlr.Handle(request, new CancellationTokenSource().Token).Result;

        //Assert
        Assert.NotNull(obj);
        Assert.NotEmpty(obj);

    }

    [Fact]
    public void CreateShortUrlCommand_Handle_Success()
    {
        CreateShortUrlCommand cmd = new CreateShortUrlCommand
        {
            Url = "https://www.newurl.com"
        };

        _ = _mockdb.Setup(i => i.Urls).ReturnsDbSet(new Url[] { new Url { OriginalUrl = "https://www.google.com", CompactUrl = "abc123" } });
        _ = _mockdb.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);
        _ = _mockhashsvc.Setup(i => i.GenerateHash()).Returns("abc432");
        CreateShortUrlCommandHandler handlr = new CreateShortUrlCommandHandler(_mockdb.Object, _mockhashids.Object, _mockhashsvc.Object);
        var obj = handlr.Handle(cmd, It.IsAny<CancellationToken>()).Result;

        Assert.NotNull(obj);
        Assert.NotEqual("Save Error", obj);
    }
}


