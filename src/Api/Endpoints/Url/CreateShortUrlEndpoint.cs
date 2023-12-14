using MediatR;
using Microsoft.AspNetCore.Mvc.Routing;
using UrlShortenerService.Api.Endpoints.Url.Requests;
using UrlShortenerService.Application.Url.Commands;
using IMapper = AutoMapper.IMapper;

namespace UrlShortenerService.Api.Endpoints.Url;

public class CreateShortUrlSummary : Summary<CreateShortUrlEndpoint>
{
    public CreateShortUrlSummary()
    {
        Summary = "Create short url from provided url";
        Description =
            "This endpoint will create a short url from provided original url.";
        Response(500, "Internal server error.");
    }
}

public class CreateShortUrlEndpoint : BaseEndpoint<CreateShortUrlRequest>
{
    public CreateShortUrlEndpoint(ISender mediator, IMapper mapper)
        : base(mediator, mapper)
    {

    }

    public override void Configure()
    {
        base.Configure();
        Post("u");
        AllowAnonymous();
        Description(
            d => d.WithTags("Url")
        );
        Summary(new CreateShortUrlSummary());
    }

    public override async Task HandleAsync(CreateShortUrlRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send<string>(
            new CreateShortUrlCommand
            {
                Url = req.Url
            },
            ct
        );

        UriBuilder uri = new UriBuilder(HttpContext.Request.Scheme, HttpContext.Request.Host.Host,HttpContext.Request.Host.Port.Value , $"{HttpContext.Request.Path.Value}/{result}");
        //string url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.Path}/{result}";
        await SendOkAsync(uri.Uri.ToString());
    }
}




