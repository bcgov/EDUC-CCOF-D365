﻿using Microsoft.Xrm.Sdk;

namespace CCOF.Infrastructure.WebAPI.Messages;

/// <summary>
/// Contains the data to update file column
/// </summary>
public sealed class DownloadFileRequest : HttpRequestMessage
{
    public DownloadFileRequest(
        D365EntityReference entityReference,
        string columnName,
        bool returnFullSizedImage = false)
    {
       

        Method = HttpMethod.Get;
        RequestUri = new Uri(
            uriString: $"{entityReference.Path}/{columnName}/$value",
            uriKind: UriKind.Relative);
      
    }
}