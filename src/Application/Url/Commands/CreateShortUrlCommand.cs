using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Common.Services;
using UrlShortenerService.Domain.Entities;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
        _ = RuleFor(v => v.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? result))
            .WithMessage("Url is not in a correct format");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;
    private readonly IHashGeneratorService _hashGeneratorService;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids, IHashGeneratorService hashGeneratorService)
    {
        _context = context;
        _hashids = hashids;
        _hashGeneratorService = hashGeneratorService;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        var longurl = request.Url;


        //Check if URL exists
        if (await _context.Urls.AnyAsync(i => i.OriginalUrl.Equals(longurl)))
        {
            return _context.Urls.Single(i => i.OriginalUrl.Equals(longurl)).CompactUrl;
        }
        string hashedurl = default!;
        bool checkurlunique = false;
        while (!checkurlunique)
        {
            hashedurl = _hashGeneratorService.GenerateHash();
            if (!await _context.Urls.AnyAsync(i => i.CompactUrl.Equals(hashedurl)))
            {
                checkurlunique = true;
                break;
            }
        }

        //Add and Save the original and hashed urls
        _ = await _context.Urls.AddAsync(new Domain.Entities.Url { CompactUrl = hashedurl, OriginalUrl = longurl });
        bool issaved = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (issaved)
        {
            return hashedurl;
        }
        return "Save Error";

    }
}


