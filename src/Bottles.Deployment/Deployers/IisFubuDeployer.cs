using System.Collections.Generic;
using FubuCore;
using Microsoft.Web.Administration;

namespace Bottles.Deployment.Deployers
{
    public class IisFubuDeployer : IDeployer<IisFubuWebsite>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBottleRepository _bottles;

        public IisFubuDeployer(IFileSystem fileSystem, IBottleRepository bottles)
        {
            _fileSystem = fileSystem;
            _bottles = bottles;
        }

        //http://www.iis.net/ConfigReference/
        public void Deploy(IDirective directive)
        {
            var direc = (IisFubuWebsite) directive;

            _fileSystem.CreateDirectory(direc.VDirPhysicalPath);

            //REVIEW: currently this is grouped and done once per deployment
            //REVIEW: so if you have N sites you get this happening N times, one by one
            //REVIEW: we may want to make this step occur outside of the actual deploy???? - surround via StructureMap and AOP'esque stuff
            var appOfflineFile = FileSystem.Combine(direc.VDirPhysicalPath, "app_offline.htm");

            _fileSystem.WriteStringToFile(appOfflineFile, "&lt;html&gt;&lt;body&gt;Application is being rebuilt&lt;/body&gt;&lt;/html&gt;");
            

            //currenly only IIS 7
            using (var iisManager = ServerManager.OpenRemote("."))
            {
                var pool = iisManager.CreateAppPool(direc.AppPool);
                pool.ManagedRuntimeVersion = "v4.0";
                pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                pool.Enable32BitAppOnWin64 = false;

                if (direc.HasCredentials())
                {
                    pool.ProcessModel.UserName = direc.Username;
                    pool.ProcessModel.Password = direc.Password;
                }

                var site = iisManager.CreateSite(direc.WebsiteName, direc.WebsitePhysicalPath, direc.Port);


                var app = site.CreateApplication(direc.VDir, direc.VDirPhysicalPath);
                app.ApplicationPoolName = direc.AppPool;
                app.DirectoryBrowsing(direc.DirectoryBrowsing);
                app.AnonAuthentication(direc.AnonAuth);
                app.BasicAuthentication(direc.BasicAuth);
                app.WindowsAuthentication(direc.WindowsAuth);
                app.MapAspNetToEverything();


                iisManager.CommitChanges();
            }


            //host bottle
            _bottles.ExplodeTo(direc.HostBottle, direc.VDirPhysicalPath);

            var bottleDest = FileSystem.Combine(direc.VDirPhysicalPath, "packages");
            direc.Bottles.Each(b =>
            {
                _bottles.CopyTo(b, bottleDest);
            });
            

            _fileSystem.DeleteFile(appOfflineFile);
        }
    }
}