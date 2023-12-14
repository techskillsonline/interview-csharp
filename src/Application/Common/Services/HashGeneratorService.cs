using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HashidsNet;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Common.Services;
internal class HashGeneratorService : IHashGeneratorService
{
    private readonly IHashids _hashids;
    private readonly int _minnumbertoencode;
    private readonly int _maxnumbertoencode;

    public HashGeneratorService(IHashids hashIds, int minNumberToEncode, int maxNumberToEncode)
    {
        _hashids = hashIds;
        _minnumbertoencode = minNumberToEncode;
        _maxnumbertoencode = maxNumberToEncode;
    }

    public string GenerateHash()
    {
        int randomnum = Random.Shared.Next(_minnumbertoencode,_maxnumbertoencode);
        return _hashids.Encode(randomnum);
    }
}

