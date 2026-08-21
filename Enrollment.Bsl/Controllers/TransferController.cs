using Enrollment.Bsl.Flow;
using LogicBuilder.App.Utils.Rules;
using LogicBuilder.App.Utils.Rules.Interfaces;
using LogicBuilder.DataContracts;
using LogicBuilder.RulesDirector;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Controllers
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController(IRulesCache rulesCache, IRulesLoader rulesLoader, IWebHostEnvironment webHostEnvironment) : ControllerBase
    {
        private readonly IRulesCache _rulesCache = rulesCache;
        private readonly IRulesLoader _rulesLoader = rulesLoader;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        [HttpPost("PostFileData")]
        public async Task<IActionResult> PostFileData([FromBody] ModuleData moduleData)
        {
            try
            {
                if (_webHostEnvironment.EnvironmentName != "Development")
                {
                    throw new InvalidOperationException(
                        "This shouldn't be invoked in non-development environments.");
                }

                await _rulesLoader.LoadRules
                (
                    new RulesModule
                    (
                        moduleData.ModuleName.ToLowerInvariant(),
                        moduleData.ResourcesStream,
                        moduleData.RulesStream
                    ),
                    _rulesCache,
                    new RulesLoaderRequest
                    (
                        "Enrollment.Bsl.Flow.Rulesets",
                        typeof(FlowActivity),
                        [
                            typeof(LogicBuilder.App.Utils.Interfaces.ITypeHelper).Assembly,
                            typeof(LogicBuilder.Forms.Parameters.Expansions.SelectExpandDefinitionParameters).Assembly,
                            typeof(Domain.Entities.UserModel).Assembly,
                            typeof(Data.Entities.User).Assembly,
                            typeof(DirectorBase).Assembly,
                            typeof(string).Assembly
                        ]
                    )
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
