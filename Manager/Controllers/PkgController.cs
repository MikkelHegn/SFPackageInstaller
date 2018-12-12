using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using SFPackageInstaller.Manager.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFPackageInstaller.Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PkgController : ControllerBase
    {
        private readonly IReliableStateManager _statemanager;

        public PkgController(IReliableStateManager stateManager)
        {
            this._statemanager = stateManager;
        }

        // GET api/pkg
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstallerPkg>>> Get()
        {
            List<InstallerPkg> result = await GetInstallerPackagesAsync();

            return new JsonResult(result);
        }

        // POST api/pkg
        [HttpPost]
        public async Task<ActionResult<InstallerPkg>> Put(InstallerPkg installerPkg)
        {
            RCAddResult result = await AddInstallerPackageAsync(installerPkg);

            switch (result)
            {
                case RCAddResult.Success:
                    return installerPkg;
                case RCAddResult.Failure:
                    return new BadRequestResult();
                case RCAddResult.KeyAlreadyExists:
                    return new ConflictResult();
                default:
                    return new BadRequestResult();
            }

        }

        private async Task<RCAddResult> AddInstallerPackageAsync(InstallerPkg pkg)
        {
            IReliableDictionary<string, InstallerPkg> packagesDictionary = await this._statemanager.GetOrAddAsync<IReliableDictionary<string, InstallerPkg>>("packages");

            using (ITransaction tx = _statemanager.CreateTransaction())
            {
                var result = await packagesDictionary.TryGetValueAsync(tx, pkg.Name);

                if (result.HasValue)
                {
                    return RCAddResult.KeyAlreadyExists;
                }

                if (!await packagesDictionary.TryAddAsync(tx, pkg.Name, pkg))
                {
                    return RCAddResult.Failure;
                }

                await tx.CommitAsync();
            }

            return RCAddResult.Success;
        }

        private async Task<List<InstallerPkg>> GetInstallerPackagesAsync()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, InstallerPkg> packagesDictionary = await this._statemanager.GetOrAddAsync<IReliableDictionary<string, InstallerPkg>>("packages");

            using (ITransaction tx = this._statemanager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, InstallerPkg>> list = await packagesDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, InstallerPkg>> enumerator = list.GetAsyncEnumerator();

                List<InstallerPkg> result = new List<InstallerPkg>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current.Value);
                }

                return (result);
            }

        }
    }
}