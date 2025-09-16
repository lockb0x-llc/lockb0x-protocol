using Microsoft.AspNetCore.Mvc;
using Lockb0x.Core;

namespace Lockb0x.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodexController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create([FromBody] object file) => StatusCode(501);

    [HttpPost("sign")]
    public IActionResult Sign([FromBody] CodexEntry entry) => StatusCode(501);

    [HttpPost("anchor")]
    public IActionResult Anchor([FromBody] CodexEntry entry) => StatusCode(501);

    [HttpPost("certify")]
    public IActionResult Certify([FromBody] CodexEntry entry) => StatusCode(501);

    [HttpPost("verify")]
    public IActionResult Verify([FromBody] CodexEntry entry) => StatusCode(501);
}
