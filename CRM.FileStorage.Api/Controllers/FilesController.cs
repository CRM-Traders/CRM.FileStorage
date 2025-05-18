using CRM.FileStorage.Api.Controllers.Base;
using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.FileStorage.Api.Controllers;

public class FilesController(IFileService fileService) : BaseController
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetFile(Guid id)
    {
        var result = await fileService.GetFileContentAsync(id);

        if (result.IsFailure)
            return ToResult(result);

        return Results.File(
            result.Value.FileContent,
            result.Value.ContentType,
            result.Value.FileName);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> UploadFile([FromForm] UploadFileRequest request)
    {
        var result = await fileService.UploadFileAsync(request);
        return ToResult(result);
    }

    [HttpPost("{id}/make-permanent")]
    [Authorize]
    [ProducesResponseType(typeof(MakePermanentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> MakeFilePermanent(Guid id)
    {
        var request = new MakePermanentRequest { FileId = id };
        var result = await fileService.MakeFilePermanentAsync(request);
        return ToResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> DeleteFile(Guid id)
    {
        var result = await fileService.DeleteFileAsync(id);
        return ToResult(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StoredFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> GetUserFiles(Guid userId, [FromQuery] FileType? fileType = null)
    {
        var result = await fileService.GetFilesByUserIdAsync(userId, fileType);
        return ToResult(result);
    }

    [HttpGet("reference/{reference}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StoredFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetFilesByReference(string reference)
    {
        var result = await fileService.GetFilesByReferenceAsync(reference);
        return ToResult(result);
    }
}