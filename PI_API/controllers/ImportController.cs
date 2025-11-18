using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PI_API.services;

namespace PI_API.controllers;

public class ImportController
{
    [HttpGet("/api/import")]
    public IActionResult Import()
    {
        return new Ok();
    }
}