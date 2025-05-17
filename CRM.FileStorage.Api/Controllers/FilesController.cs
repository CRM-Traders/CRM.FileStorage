using CRM.FileStorage.Api.Controllers.Base;
using CRM.FileStorage.Application.Common.Models;
using CRM.FileStorage.Application.Interfaces.Services;
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

    [HttpPost("{id}/make-permanent")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MakePermanentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> MakeFilePermanent(Guid id)
    {
        var request = new MakePermanentRequest { FileId = id };
        var result = await fileService.MakeFilePermanentAsync(request);
        return ToResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> DeleteFile(Guid id)
    {
        var result = await fileService.DeleteFileAsync(id);

        if (result.IsSuccess)
            return TypedResults.NoContent();

        return ToResult(result);
    }
}