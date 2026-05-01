using ICEBOM.Core.Domain.Models.Odoo;

namespace ICEBOM.Core.Domain.Repositories
{
    public interface IOdooRepository
    {
        OdooProductInfo GetProduct(string reference);
        OdooBomInfo GetBom(string productReference);

        OdooProductInfo CreateProduct(string reference);
        OdooProductInfo UpdateProduct(string reference);

        OdooBomInfo CreateBom(string productReference);
        OdooBomInfo UpdateBom(string productReference);

        OdooUnitInfo GetUnit(string unitName);

        OdooCategoryInfo GetCategory(string categoryName);
    }
}