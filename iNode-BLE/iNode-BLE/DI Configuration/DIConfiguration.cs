using Autofac;
using ERGBLE;
using ERGBLE.Models;
using ERGBLE.Models.Builders;
using ERGBLE.Models.Selectors;
using ERGBLE.Services;
using ERGBLE.ViewModels;
using ERGBLE.ViewModels.Commands;
using ERGBLE.ViewModels.Commands.Builders;
using ERGBLE.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERGBLE
{
    public static class DIConfiguration
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            #region Models
            builder.RegisterType<MeasurementDataHT>()
                .As<IMeasurementData>()
                .Keyed<IMeasurementData>(SensorType.HT);
            builder.RegisterType<MeasurementDataT>()
                .As<IMeasurementData>()
                .Keyed<IMeasurementData>(SensorType.T);

            builder.RegisterGeneric(typeof(ImplementationSelector<,>)).As(typeof(IImplementationSelector<,>));

            builder.RegisterType<SensorInfoBuilder>().AsSelf();
            builder.RegisterType<MeasurementDataBuilder>().AsSelf();
            #endregion

            #region Services
            builder.RegisterType<SecondsTimer>().As<ISecondsTimer>();

            builder.RegisterType<DeviceInfoReader>().As<IDeviceInfoReader>();
            builder.RegisterType<DeviceDataReader>().As<IDeviceDataReader>();
            builder.RegisterType<DeviceScanner>().As<IDeviceScanner>().SingleInstance();
            builder.RegisterType<DeviceConnector>().As<IDeviceConnector>();
            builder.RegisterType<DataConverter>().As<IDataConverter>();
            builder.RegisterType<DataParser>().As<IDataParser>();
            #endregion

            #region ViewModels
            builder.RegisterType<DevicesViewModel>().As<IDevicesViewModel>().SingleInstance();

            builder.RegisterGeneric(typeof(CommandsBuilder<,>));
            builder.RegisterType<ScanForDevicesCommand>().AsSelf();
            builder.RegisterType<SaveRecordsCommand>().AsSelf();
            #endregion

            #region Views
            builder.RegisterType<MainPage>().AsSelf();
            #endregion

            return builder.Build();
        }
    }
}
