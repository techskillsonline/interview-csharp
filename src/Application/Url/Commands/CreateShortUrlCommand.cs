using FluentValidation;
using HashidsNet;
using MediatR;
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
        //RuleFor<CreateShortUrlCommand>(i => i).Custom((i, j) => Uri.TryCreate(i.Url, UriKind.Absolute, out var result))
        //    .Custom()
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

        //Validate if url is a Valid Schema
        if (Uri.TryCreate(longurl, UriKind.Absolute, out var uri))
        {
            //Check if URL exists
            if (_context.Urls.Any(i => i.OriginalUrl.Equals(uri)))
            {
                return _context.Urls.Single(i => i.OriginalUrl.Equals(uri)).CompactUrl;
            }
            string hashedurl = default!;
            bool checkurlunique = false;
            while (!checkurlunique)
            {
                hashedurl = _hashGeneratorService.GenerateHash();
                if (!_context.Urls.Any(i => i.CompactUrl.Equals(hashedurl)))
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

        return "Not a Valid URL";
    }
}


