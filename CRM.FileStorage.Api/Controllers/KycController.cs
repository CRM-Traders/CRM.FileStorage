using CRM.FileStorage.Api.Controllers.Base;
using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.FileStorage.Api.Controllers;

public class KycController(
    IKycService kycService,
    IHttpContextAccessor httpContextAccessor)
    : BaseController
{
    [HttpPost("process")]
    [Authorize]
    [ProducesResponseType(typeof(CreateKycProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> CreateKycProcess()
    {
        var request = new CreateKycProcessRequest();
        var result = await kycService.CreateKycProcessAsync(request);
        return ToResult(result);
    }

    [HttpGet("process/{idOrToken}")]
    [ProducesResponseType(typeof(KycProcessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetKycProcess(string idOrToken)
    {
        var result = await kycService.GetKycProcessAsync(idOrToken);
        return ToResult(result);
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadKycFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> UploadKycFile([FromForm] UploadKycFileRequest request)
    {
        var result = await kycService.UploadKycFileAsync(request);
        return ToResult(result);
    }

    [HttpGet("qr/{idOrToken}")]
    [ProducesResponseType(typeof(QrCodeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetKycQrCode(string idOrToken)
    {
        var baseUrl =
            $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}";
        var result = await kycService.GetKycQrCodeAsync(idOrToken, baseUrl);
        return ToResult(result);
    }

    [HttpPost("verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VerifyKycProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> VerifyKycProcess(VerifyKycProcessRequest request)
    {
        var result = await kycService.VerifyKycProcessAsync(request);
        return ToResult(result);
    }
}