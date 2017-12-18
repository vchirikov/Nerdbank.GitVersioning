﻿namespace Nerdbank.GitVersioning
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Validation;

    /// <summary>
    /// Describes the various versions and options required for the build.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class VersionOptions : IEquatable<VersionOptions>
    {
        /// <summary>
        /// Default value for <see cref="VersionPrecision"/>.
        /// </summary>
        public const VersionPrecision DefaultVersionPrecision = VersionPrecision.Minor;

        /// <summary>
        /// The default value for the <see cref="SemVer1NumericIdentifierPaddingOrDefault"/> property.
        /// </summary>
        private const int DefaultSemVer1NumericIdentifierPadding = 4;

        /// <summary>
        /// Gets or sets the default version to use.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SemanticVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the version to use particularly for the <see cref="AssemblyVersionAttribute"/>
        /// instead of the default <see cref="Version"/>.
        /// </summary>
        /// <value>An instance of <see cref="System.Version"/> or <c>null</c> to simply use the default <see cref="Version"/>.</value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AssemblyVersionOptions AssemblyVersion { get; set; }

        /// <summary>
        /// Gets the version to use particularly for the <see cref="AssemblyVersionAttribute"/>
        /// instead of the default <see cref="Version"/>.
        /// </summary>
        /// <value>An instance of <see cref="System.Version"/> or <c>null</c> to simply use the default <see cref="Version"/>.</value>
        [JsonIgnore]
        public AssemblyVersionOptions AssemblyVersionOrDefault => this.AssemblyVersion ?? AssemblyVersionOptions.DefaultInstance;

        /// <summary>
        /// Gets or sets a number to add to the git height when calculating the <see cref="Version.Build"/> number.
        /// </summary>
        /// <value>Any integer (0, positive, or negative).</value>
        /// <remarks>
        /// An error will result if this value is negative with such a magnitude as to exceed the git height,
        /// resulting in a negative build number.
        /// </remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? BuildNumberOffset { get; set; }

        /// <summary>
        /// Gets a number to add to the git height when calculating the <see cref="Version.Build"/> number.
        /// </summary>
        /// <value>Any integer (0, positive, or negative).</value>
        /// <remarks>
        /// An error will result if this value is negative with such a magnitude as to exceed the git height,
        /// resulting in a negative build number.
        /// </remarks>
        [JsonIgnore]
        public int BuildNumberOffsetOrDefault => this.BuildNumberOffset ?? 0;

        /// <summary>
        /// Gets or sets the minimum number of digits to use for numeric identifiers in SemVer 1.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? SemVer1NumericIdentifierPadding { get; set; }

        /// <summary>
        /// Gets the minimum number of digits to use for numeric identifiers in SemVer 1.
        /// </summary>
        [JsonIgnore]
        public int SemVer1NumericIdentifierPaddingOrDefault => this.SemVer1NumericIdentifierPadding ?? DefaultSemVer1NumericIdentifierPadding;

        /// <summary>
        /// Gets or sets the options around NuGet version strings
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public NuGetPackageVersionOptions NuGetPackageVersion { get; set; }

        /// <summary>
        /// Gets the options around NuGet version strings
        /// </summary>
        [JsonIgnore]
        public NuGetPackageVersionOptions NuGetPackageVersionOrDefault => this.NuGetPackageVersion ?? NuGetPackageVersionOptions.DefaultInstance;

        /// <summary>
        /// Gets or sets an array of regular expressions that describes branch or tag names that should
        /// be built with PublicRelease=true as the default value on build servers.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] PublicReleaseRefSpec { get; set; }

        /// <summary>
        /// Gets an array of regular expressions that describes branch or tag names that should
        /// be built with PublicRelease=true as the default value on build servers.
        /// </summary>
        [JsonIgnore]
        public string[] PublicReleaseRefSpecOrDefault => this.PublicReleaseRefSpec ?? new string[0];

        /// <summary>
        /// Gets or sets the options around cloud build.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CloudBuildOptions CloudBuild { get; set; }

        /// <summary>
        /// Gets the options around cloud build.
        /// </summary>
        [JsonIgnore]
        public CloudBuildOptions CloudBuildOrDefault => this.CloudBuild ?? CloudBuildOptions.DefaultInstance;

        /// <summary>
        /// Gets or sets a value indicating whether this options object should inherit from an ancestor any settings that are not explicitly set in this one.
        /// </summary>
        /// <remarks>
        /// When this is <c>true</c>, this object may not completely describe the options to be applied.
        /// </remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Inherit { get; set; }

        /// <summary>
        /// Gets the debugger display for this instance.
        /// </summary>
        private string DebuggerDisplay => this.Version?.ToString() ?? (this.Inherit ? "Inheriting version info" : "(missing version)");

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionOptions"/> class
        /// with <see cref="Version"/> initialized with the specified parameters.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="unstableTag">The prerelease tag, if any.</param>
        /// <returns>The new instance of <see cref="VersionOptions"/>.</returns>
        public static VersionOptions FromVersion(Version version, string unstableTag = null)
        {
            return new VersionOptions
            {
                Version = new SemanticVersion(version, unstableTag),
            };
        }

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> to use based on certain requirements.
        /// </summary>
        /// <param name="includeDefaults"></param>
        /// <returns>The serializer settings to use.</returns>
        public static JsonSerializerSettings GetJsonSettings(bool includeDefaults = false)
        {
            return new JsonSerializerSettings
            {
                Converters = new JsonConverter[] {
                    new VersionConverter(),
                    new SemanticVersionJsonConverter(),
                    new AssemblyVersionOptionsConverter(includeDefaults),
                    new StringEnumConverter() { CamelCaseText = true },
                },
                ContractResolver = includeDefaults ? VersionOptionsContractResolver.IncludeDefaultsContractResolver : VersionOptionsContractResolver.ExcludeDefaultsContractResolver,
                Formatting = Formatting.Indented,
            };
        }

        /// <summary>
        /// Checks equality against another object.
        /// </summary>
        /// <param name="obj">The other instance.</param>
        /// <returns><c>true</c> if the instances have equal values; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as VersionOptions);
        }

        /// <summary>
        /// Gets a hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

        /// <summary>
        /// Checks equality against another instance of this class.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><c>true</c> if the instances have equal values; <c>false</c> otherwise.</returns>
        public bool Equals(VersionOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);

        /// <summary>
        /// Gets a value indicating whether <see cref="Version"/> is
        /// set and the only property on this class that is set.
        /// </summary>
        internal bool IsDefaultVersionTheOnlyPropertySet
        {
            get
            {
                return this.Version != null
                    && this.AssemblyVersion == null
                    && this.CloudBuild.IsDefault
                    && this.BuildNumberOffset == 0
                    && !this.SemVer1NumericIdentifierPadding.HasValue
                    && !this.Inherit;
            }
        }

        /// <summary>
        /// The class that contains settings for the <see cref="NuGetPackageVersion" /> property.
        /// </summary>
        public class NuGetPackageVersionOptions : IEquatable<NuGetPackageVersionOptions>
        {
            /// <summary>
            /// The default (uninitialized) instance.
            /// </summary>
            internal static readonly NuGetPackageVersionOptions DefaultInstance = new NuGetPackageVersionOptions(isReadOnly: true)
            {
                semVer = 1.0f,
            };

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly bool isReadOnly;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private float? semVer;

            /// <summary>
            /// Initializes a new instance of the <see cref="NuGetPackageVersionOptions" /> class.
            /// </summary>
            public NuGetPackageVersionOptions()
                : this(isReadOnly: false)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="NuGetPackageVersionOptions" /> class.
            /// </summary>
            protected NuGetPackageVersionOptions(bool isReadOnly)
            {
                this.isReadOnly = isReadOnly;
            }

            /// <summary>
            /// Gets or sets the version of SemVer (e.g. 1 or 2) that should be used when generating the package version.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public float? SemVer
            {
                get => this.semVer;
                set => this.SetIfNotReadOnly(ref this.semVer, value);
            }

            /// <summary>
            /// Gets the version of SemVer (e.g. 1 or 2) that should be used when generating the package version.
            /// </summary>
            [JsonIgnore]
            public float? SemVerOrDefault => this.SemVer ?? DefaultInstance.SemVer;

            /// <inheritdoc />
            public override bool Equals(object obj) => this.Equals(obj as NuGetPackageVersionOptions);

            /// <inheritdoc />
            public bool Equals(NuGetPackageVersionOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);

            /// <inheritdoc />
            public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

            /// <summary>
            /// Gets a value indicating whether this instance is equivalent to the default instance.
            /// </summary>
            internal bool IsDefault => this.Equals(DefaultInstance);

            /// <summary>
            /// Sets the value of a field if this instance is not marked as read only.
            /// </summary>
            /// <typeparam name="T">The type of the value stored by the field.</typeparam>
            /// <param name="field">The field to change.</param>
            /// <param name="value">The value to set.</param>
            private void SetIfNotReadOnly<T>(ref T field, T value)
            {
                Verify.Operation(!this.isReadOnly, "This instance is read only.");
                field = value;
            }

            internal class EqualWithDefaultsComparer : IEqualityComparer<NuGetPackageVersionOptions>
            {
                internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

                private EqualWithDefaultsComparer() { }

                /// <inheritdoc />
                public bool Equals(NuGetPackageVersionOptions x, NuGetPackageVersionOptions y)
                {
                    if (x == null ^ y == null)
                    {
                        return false;
                    }

                    if (x == null)
                    {
                        return true;
                    }

                    return x.SemVerOrDefault == y.SemVerOrDefault;
                }

                /// <inheritdoc />
                public int GetHashCode(NuGetPackageVersionOptions obj)
                {
                    return obj.SemVerOrDefault.GetHashCode();
                }
            }
        }

        /// <summary>
        /// Describes the details of how the AssemblyVersion value will be calculated.
        /// </summary>
        public class AssemblyVersionOptions : IEquatable<AssemblyVersionOptions>
        {
            /// <summary>
            /// The default (uninitialized) instance.
            /// </summary>
            internal static readonly AssemblyVersionOptions DefaultInstance = new AssemblyVersionOptions(isReadOnly: true)
            {
                precision = DefaultVersionPrecision,
            };

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly bool isReadOnly;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Version version;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private VersionPrecision? precision;

            /// <summary>
            /// Initializes a new instance of the <see cref="AssemblyVersionOptions"/> class.
            /// </summary>
            public AssemblyVersionOptions()
                : this(isReadOnly: false)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AssemblyVersionOptions"/> class.
            /// </summary>
            /// <param name="version">The assembly version (with major.minor components).</param>
            /// <param name="precision">The additional version precision to add toward matching the AssemblyFileVersion.</param>
            public AssemblyVersionOptions(Version version, VersionPrecision? precision = null)
                : this(isReadOnly: false)
            {
                this.Version = version;
                this.Precision = precision;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AssemblyVersionOptions"/> class.
            /// </summary>
            protected AssemblyVersionOptions(bool isReadOnly)
            {
                this.isReadOnly = isReadOnly;
            }

            /// <summary>
            /// Gets or sets the major.minor components of the assembly version.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Version Version
            {
                get => this.version;
                set => this.SetIfNotReadOnly(ref this.version, value);
            }

            /// <summary>
            /// Gets or sets the additional version precision to add toward matching the AssemblyFileVersion.
            /// </summary>
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public VersionPrecision? Precision
            {
                get => this.precision;
                set => this.SetIfNotReadOnly(ref this.precision, value);
            }

            /// <summary>
            /// Gets the additional version precision to add toward matching the AssemblyFileVersion.
            /// </summary>
            [JsonIgnore]
            public VersionPrecision PrecisionOrDefault => this.Precision ?? DefaultVersionPrecision;

            /// <inheritdoc />
            public override bool Equals(object obj) => this.Equals(obj as AssemblyVersionOptions);

            /// <inheritdoc />
            public bool Equals(AssemblyVersionOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);

            /// <inheritdoc />
            public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

            /// <summary>
            /// Gets a value indicating whether this instance is equivalent to the default instance.
            /// </summary>
            internal bool IsDefault => this.Equals(DefaultInstance);

            /// <summary>
            /// Sets the value of a field if this instance is not marked as read only.
            /// </summary>
            /// <typeparam name="T">The type of the value stored by the field.</typeparam>
            /// <param name="field">The field to change.</param>
            /// <param name="value">The value to set.</param>
            private void SetIfNotReadOnly<T>(ref T field, T value)
            {
                Verify.Operation(!this.isReadOnly, "This instance is read only.");
                field = value;
            }

            internal class EqualWithDefaultsComparer : IEqualityComparer<AssemblyVersionOptions>
            {
                internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

                private EqualWithDefaultsComparer() { }

                /// <inheritdoc />
                public bool Equals(AssemblyVersionOptions x, AssemblyVersionOptions y)
                {
                    if (x == null ^ y == null)
                    {
                        return false;
                    }

                    if (x == null)
                    {
                        return true;
                    }

                    return EqualityComparer<Version>.Default.Equals(x.Version, y.Version)
                        && x.PrecisionOrDefault == y.PrecisionOrDefault;
                }

                /// <inheritdoc />
                public int GetHashCode(AssemblyVersionOptions obj)
                {
                    return (obj.Version?.GetHashCode() ?? 0) + (int)obj.PrecisionOrDefault;
                }
            }
        }

        /// <summary>
        /// Options that are applicable specifically to cloud builds (e.g. VSTS, AppVeyor, TeamCity)
        /// </summary>
        public class CloudBuildOptions : IEquatable<CloudBuildOptions>
        {
            /// <summary>
            /// The default (uninitialized) instance.
            /// </summary>
            internal static readonly CloudBuildOptions DefaultInstance = new CloudBuildOptions(isReadOnly: true)
            {
                setAllVariables = false,
                setVersionVariables = true,
            };

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly bool isReadOnly;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool? setAllVariables;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool? setVersionVariables;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private CloudBuildNumberOptions buildNumber;

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildOptions"/> class.
            /// </summary>
            public CloudBuildOptions()
                : this(false)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildOptions"/> class.
            /// </summary>
            protected CloudBuildOptions(bool isReadOnly)
            {
                this.isReadOnly = isReadOnly;
            }

            /// <summary>
            /// Gets or sets a value indicating whether to elevate all build properties to cloud build variables prefaced with "NBGV_".
            /// </summary>
            public bool? SetAllVariables
            {
                get => this.setAllVariables;
                set => this.SetIfNotReadOnly(ref this.setAllVariables, value);
            }

            /// <summary>
            /// Gets or sets a value indicating whether to elevate certain calculated version build properties to cloud build variables.
            /// </summary>
            public bool? SetVersionVariables
            {
                get => this.setVersionVariables;
                set => this.SetIfNotReadOnly(ref this.setVersionVariables, value);
            }

            /// <summary>
            /// Gets a value indicating whether to elevate all build properties to cloud build variables prefaced with "NBGV_".
            /// </summary>
            [JsonIgnore]
            public bool SetAllVariablesOrDefault => this.SetAllVariables ?? DefaultInstance.SetAllVariables.Value;

            /// <summary>
            /// Gets a value indicating whether to elevate certain calculated version build properties to cloud build variables.
            /// </summary>
            [JsonIgnore]
            public bool SetVersionVariablesOrDefault => this.SetVersionVariables ?? DefaultInstance.SetVersionVariables.Value;

            /// <summary>
            /// Gets or sets options around how and whether to set the build number preset by the cloud build with one enriched with version information.
            /// </summary>
            public CloudBuildNumberOptions BuildNumber
            {
                get => this.buildNumber;
                set => this.SetIfNotReadOnly(ref this.buildNumber, value);
            }

            /// <summary>
            /// Gets options around how and whether to set the build number preset by the cloud build with one enriched with version information.
            /// </summary>
            [JsonIgnore]
            public CloudBuildNumberOptions BuildNumberOrDefault => this.BuildNumber ?? CloudBuildNumberOptions.DefaultInstance;

            /// <inheritdoc />
            public override bool Equals(object obj) => this.Equals(obj as CloudBuildOptions);

            /// <inheritdoc />
            public bool Equals(CloudBuildOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);

            /// <inheritdoc />
            public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

            /// <summary>
            /// Gets a value indicating whether this instance is equivalent to the default instance.
            /// </summary>
            internal bool IsDefault => this.Equals(DefaultInstance);

            /// <summary>
            /// Sets the value of a field if this instance is not marked as read only.
            /// </summary>
            /// <typeparam name="T">The type of the value stored by the field.</typeparam>
            /// <param name="field">The field to change.</param>
            /// <param name="value">The value to set.</param>
            private void SetIfNotReadOnly<T>(ref T field, T value)
            {
                Verify.Operation(!this.isReadOnly, "This instance is read only.");
                field = value;
            }

            internal class EqualWithDefaultsComparer : IEqualityComparer<CloudBuildOptions>
            {
                internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

                private EqualWithDefaultsComparer() { }

                /// <inheritdoc />
                public bool Equals(CloudBuildOptions x, CloudBuildOptions y)
                {
                    if (x == null ^ y == null)
                    {
                        return false;
                    }

                    if (x == null)
                    {
                        return true;
                    }

                    return x.SetVersionVariablesOrDefault == y.SetVersionVariablesOrDefault
                        && x.SetAllVariablesOrDefault == y.SetAllVariablesOrDefault
                        && CloudBuildNumberOptions.EqualWithDefaultsComparer.Singleton.Equals(x.BuildNumberOrDefault, y.BuildNumberOrDefault);
                }

                /// <inheritdoc />
                public int GetHashCode(CloudBuildOptions obj)
                {
                    return (obj.SetVersionVariablesOrDefault ? 1 : 0)
                        + (obj.SetAllVariablesOrDefault ? 1 : 0)
                        + obj.BuildNumberOrDefault.GetHashCode();
                }
            }
        }

        /// <summary>
        /// Override the build number preset by the cloud build with one enriched with version information.
        /// </summary>
        public class CloudBuildNumberOptions : IEquatable<CloudBuildNumberOptions>
        {
            /// <summary>
            /// The default (uninitialized) instance.
            /// </summary>
            internal static readonly CloudBuildNumberOptions DefaultInstance = new CloudBuildNumberOptions(isReadOnly: true)
            {
                enabled = false,
            };

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly bool isReadOnly;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool? enabled;

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildNumberOptions"/> class.
            /// </summary>
            public CloudBuildNumberOptions()
                : this(isReadOnly: false)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildNumberOptions"/> class.
            /// </summary>
            protected CloudBuildNumberOptions(bool isReadOnly)
            {
                this.isReadOnly = isReadOnly;
            }

            /// <summary>
            /// Gets or sets a value indicating whether to override the build number preset by the cloud build.
            /// </summary>
            public bool? Enabled
            {
                get => this.enabled;
                set => this.SetIfNotReadOnly(ref this.enabled, value);
            }

            /// <summary>
            /// Gets a value indicating whether to override the build number preset by the cloud build.
            /// </summary>
            [JsonIgnore]
            public bool EnabledOrDefault => this.Enabled ?? DefaultInstance.Enabled.Value;

            /// <summary>
            /// Gets or sets when and where to include information about the git commit being built.
            /// </summary>
            public CloudBuildNumberCommitIdOptions IncludeCommitId { get; set; }

            /// <summary>
            /// Gets when and where to include information about the git commit being built.
            /// </summary>
            [JsonIgnore]
            public CloudBuildNumberCommitIdOptions IncludeCommitIdOrDefault => this.IncludeCommitId ?? CloudBuildNumberCommitIdOptions.DefaultInstance;

            /// <inheritdoc />
            public override bool Equals(object obj) => this.Equals(obj as CloudBuildNumberOptions);

            /// <inheritdoc />
            public bool Equals(CloudBuildNumberOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);

            /// <inheritdoc />
            public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

            /// <summary>
            /// Gets a value indicating whether this instance is equivalent to the default instance.
            /// </summary>
            internal bool IsDefault => this.Equals(DefaultInstance);

            /// <summary>
            /// Sets the value of a field if this instance is not marked as read only.
            /// </summary>
            /// <typeparam name="T">The type of the value stored by the field.</typeparam>
            /// <param name="field">The field to change.</param>
            /// <param name="value">The value to set.</param>
            private void SetIfNotReadOnly<T>(ref T field, T value)
            {
                Verify.Operation(!this.isReadOnly, "This instance is read only.");
                field = value;
            }

            internal class EqualWithDefaultsComparer : IEqualityComparer<CloudBuildNumberOptions>
            {
                internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

                private EqualWithDefaultsComparer() { }

                /// <inheritdoc />
                public bool Equals(CloudBuildNumberOptions x, CloudBuildNumberOptions y)
                {
                    if (x == null ^ y == null)
                    {
                        return false;
                    }

                    if (x == null)
                    {
                        return true;
                    }

                    return x.EnabledOrDefault == y.EnabledOrDefault
                        && CloudBuildNumberCommitIdOptions.EqualWithDefaultsComparer.Singleton.Equals(x.IncludeCommitIdOrDefault, y.IncludeCommitIdOrDefault);
                }

                /// <inheritdoc />
                public int GetHashCode(CloudBuildNumberOptions obj)
                {
                    return obj.EnabledOrDefault ? 1 : 0
                        + obj.IncludeCommitIdOrDefault.GetHashCode();
                }
            }
        }

        /// <summary>
        /// Describes when and where to include information about the git commit being built.
        /// </summary>
        public class CloudBuildNumberCommitIdOptions : IEquatable<CloudBuildNumberCommitIdOptions>
        {
            /// <summary>
            /// The default (uninitialized) instance.
            /// </summary>
            internal static readonly CloudBuildNumberCommitIdOptions DefaultInstance = new CloudBuildNumberCommitIdOptions(isReadOnly: true)
            {
                when = CloudBuildNumberCommitWhen.NonPublicReleaseOnly,
                where = CloudBuildNumberCommitWhere.BuildMetadata,
            };

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly bool isReadOnly;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private CloudBuildNumberCommitWhen? when;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private CloudBuildNumberCommitWhere? where;

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildNumberCommitIdOptions"/> class.
            /// </summary>
            public CloudBuildNumberCommitIdOptions()
                : this(isReadOnly: false)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CloudBuildNumberCommitIdOptions"/> class.
            /// </summary>
            protected CloudBuildNumberCommitIdOptions(bool isReadOnly)
            {
                this.isReadOnly = isReadOnly;
            }

            /// <summary>
            /// Gets or sets the conditions when the commit ID is included in the build number.
            /// </summary>
            public CloudBuildNumberCommitWhen? When
            {
                get => this.when;
                set => this.SetIfNotReadOnly(ref this.when, value);
            }

            /// <summary>
            /// Gets the conditions when the commit ID is included in the build number.
            /// </summary>
            [JsonIgnore]
            public CloudBuildNumberCommitWhen WhenOrDefault => this.When ?? DefaultInstance.When.Value;

            /// <summary>
            /// Gets or sets the position to include the commit ID information.
            /// </summary>
            public CloudBuildNumberCommitWhere? Where
            {
                get => this.where;
                set => this.SetIfNotReadOnly(ref this.where, value);
            }

            /// <summary>
            /// Gets the position to include the commit ID information.
            /// </summary>
            [JsonIgnore]
            public CloudBuildNumberCommitWhere WhereOrDefault => this.Where ?? DefaultInstance.Where.Value;

            /// <inheritdoc />
            public override bool Equals(object obj) => this.Equals(obj as CloudBuildNumberCommitIdOptions);

            /// <inheritdoc />
            public bool Equals(CloudBuildNumberCommitIdOptions other) => EqualWithDefaultsComparer.Singleton.Equals(this, other);
            /// <inheritdoc />
            public override int GetHashCode() => EqualWithDefaultsComparer.Singleton.GetHashCode(this);

            /// <summary>
            /// Gets a value indicating whether this instance is equivalent to the default instance.
            /// </summary>
            internal bool IsDefault => this.Equals(DefaultInstance);

            /// <summary>
            /// Sets the value of a field if this instance is not marked as read only.
            /// </summary>
            /// <typeparam name="T">The type of the value stored by the field.</typeparam>
            /// <param name="field">The field to change.</param>
            /// <param name="value">The value to set.</param>
            private void SetIfNotReadOnly<T>(ref T field, T value)
            {
                Verify.Operation(!this.isReadOnly, "This instance is read only.");
                field = value;
            }

            internal class EqualWithDefaultsComparer : IEqualityComparer<CloudBuildNumberCommitIdOptions>
            {
                internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

                private EqualWithDefaultsComparer() { }

                /// <inheritdoc />
                public bool Equals(CloudBuildNumberCommitIdOptions x, CloudBuildNumberCommitIdOptions y)
                {
                    if (x == null ^ y == null)
                    {
                        return false;
                    }

                    if (x == null)
                    {
                        return true;
                    }

                    return x.WhenOrDefault == y.WhenOrDefault
                        && x.WhereOrDefault == y.WhereOrDefault;
                }

                /// <inheritdoc />
                public int GetHashCode(CloudBuildNumberCommitIdOptions obj)
                {
                    return (int)obj.WhereOrDefault + (int)obj.WhenOrDefault * 0x10;
                }
            }
        }

        private class EqualWithDefaultsComparer : IEqualityComparer<VersionOptions>
        {
            internal static readonly EqualWithDefaultsComparer Singleton = new EqualWithDefaultsComparer();

            private EqualWithDefaultsComparer() { }

            /// <inheritdoc />
            public bool Equals(VersionOptions x, VersionOptions y)
            {
                if (x == null ^ y == null)
                {
                    return false;
                }

                if (x == null)
                {
                    return true;
                }

                return EqualityComparer<SemanticVersion>.Default.Equals(x.Version, y.Version)
                    && AssemblyVersionOptions.EqualWithDefaultsComparer.Singleton.Equals(x.AssemblyVersionOrDefault, y.AssemblyVersionOrDefault)
                    && NuGetPackageVersionOptions.EqualWithDefaultsComparer.Singleton.Equals(x.NuGetPackageVersionOrDefault, y.NuGetPackageVersionOrDefault)
                    && CloudBuildOptions.EqualWithDefaultsComparer.Singleton.Equals(x.CloudBuildOrDefault, y.CloudBuildOrDefault)
                    && x.BuildNumberOffset == y.BuildNumberOffset;
            }

            /// <inheritdoc />
            public int GetHashCode(VersionOptions obj)
            {
                return obj.Version?.GetHashCode() ?? 0;
            }
        }

        /// <summary>
        /// The last component to control in a 4 integer version.
        /// </summary>
        public enum VersionPrecision
        {
            /// <summary>
            /// The first integer is the last number set. The rest will be zeros.
            /// </summary>
            Major,

            /// <summary>
            /// The second integer is the last number set. The rest will be zeros.
            /// </summary>
            Minor,

            /// <summary>
            /// The third integer is the last number set. The fourth will be zero.
            /// </summary>
            Build,

            /// <summary>
            /// All four integers will be set.
            /// </summary>
            Revision,
        }

        /// <summary>
        /// The conditions a commit ID is included in a cloud build number.
        /// </summary>
        public enum CloudBuildNumberCommitWhen
        {
            /// <summary>
            /// Always include the commit information in the cloud Build Number.
            /// </summary>
            Always,

            /// <summary>
            /// Only include the commit information when building a non-PublicRelease.
            /// </summary>
            NonPublicReleaseOnly,

            /// <summary>
            /// Never include the commit information.
            /// </summary>
            Never,
        }

        /// <summary>
        /// The position a commit ID can appear in a cloud build number.
        /// </summary>
        public enum CloudBuildNumberCommitWhere
        {
            /// <summary>
            /// The commit ID appears in build metadata (e.g. +ga1b2c3).
            /// </summary>
            BuildMetadata,

            /// <summary>
            /// The commit ID appears as the 4th integer in the version (e.g. 1.2.3.23523).
            /// </summary>
            FourthVersionComponent,
        }
    }
}
