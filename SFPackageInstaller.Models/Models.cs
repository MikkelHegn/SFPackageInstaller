using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace SFPackageInstaller.Manager.Models
{
    [DataContract]
    public sealed class InstallerPkg
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public ImmutableList<string> CompatibleOS { get; set; }
        [DataMember] public string DownloadURI { get; set; }
        [DataMember] public string ExecutionScript { get; set; }
        [DataMember] public string Arguments { get; set; }
        [DataMember] public ImmutableList<string> Targets { get; set; }
    }

    [DataContract]
    public class InstallerPkgStatus
    {
        [DataMember] public string InstallerPackageName { get; set; }
        [DataMember] public ImmutableList<string> NodeId { get; set; }
        [DataMember] public DeploymentStatus DeploymentStatus { get; set; }
    }

    [DataContract]
    public class Node
    {
        [DataMember] public Guid NodeId { get; set; }
        [DataMember] public string NodeName { get; set; }
        [DataMember] public DeploymentStatus DeploymentStatus { get; set; }
        [DataMember] public string LogLocation { get; set; }
    }

    public enum DeploymentStatus
    {
        NotDeployed = 0,
        Deploying = 1,
        DeployedSuccessfully = 2,
        Failed = 3
    }

    public enum RCAddResult
    {
        Success,
        Failure,
        KeyAlreadyExists
    }

}
