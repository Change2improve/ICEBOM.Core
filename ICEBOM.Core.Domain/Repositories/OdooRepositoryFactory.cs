using ICEBOM.Core.Domain.Enums;
using ICEBOM.Core.Domain.Models;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Odoo.Auth;
using ICEBOM.Core.Domain.Odoo.Repositories;

namespace ICEBOM.Core.Domain.Repositories
{
    public class OdooRepositoryFactory
    {
        public static IOdooRepositoryAsync Create(ICEBOMOdooModeConfig odooModeConfig, ICEBOMFakeOdooConfig fakeOdooConfig, OdooSession? session, OdooExecutionOptions executionOptions)
        {
            if (odooModeConfig.Mode == ICEBOMOdooModeEnum.Real)
            {
                if (session == null)
                    throw new InvalidOperationException("OdooSession es obligatorio cuando OdooMode=Real.");

                return new OdooRepository(session, executionOptions);
            }

            return new FakeOdooRepository(fakeOdooConfig);
        }
    }
}