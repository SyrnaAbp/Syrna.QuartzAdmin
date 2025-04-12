using Quartz.Impl;
using Quartz;
using System;

namespace Syrna.QuartzAdmin;

public static class QuartzDbProperties
{
    public const string DbTablePrefix = "qrtz_";

    public const string DbSchema = "quartz";

    public const string ConnectionStringName = "Quartz";
}