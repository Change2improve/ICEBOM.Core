using System.Text.Json;

using ICEBOM.Core.Domain.Constants;
using ICEBOM.Core.Domain.Models.Odoo;
using ICEBOM.Core.Domain.Odoo.Auth;
using ICEBOM.Core.Domain.Odoo.JsonRpc;
using ICEBOM.Core.Domain.Repositories;

namespace ICEBOM.Core.Domain.Odoo.Repositories
{
    public class OdooRepository : IOdooRepositoryAsync
    {
        private readonly OdooSession _session;
        private readonly OdooJsonRpcClient _client;

        private readonly OdooExecutionOptions _executionOptions;

        public OdooRepository(OdooSession session, OdooExecutionOptions? executionOptions = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _executionOptions = executionOptions ?? new OdooExecutionOptions();
            _client = new OdooJsonRpcClient(session.Url);
        }

        public async Task<OdooProductInfo> GetProductAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return new OdooProductInfo
                {
                    Id = 0,
                    Reference = string.Empty
                };
            }

            var domain = new object[]
            {
                new object[]
                {
                    OdooProductConstants.Fields.DefaultCode,
                    "=",
                    reference.Trim()
                }
            };

            var fields = new object[]
            {
                OdooProductConstants.Fields.Id,
                OdooProductConstants.Fields.Name,
                OdooProductConstants.Fields.DisplayName,
                OdooProductConstants.Fields.DefaultCode,
                OdooProductConstants.Fields.ProductTemplateId,
                OdooProductConstants.Fields.CategoryId,
                OdooProductConstants.Fields.UnitId
            };

            var kwargs = new Dictionary<string, object>
            {
                [OdooJsonRpcParameterFields.Fields] = fields,
                [OdooJsonRpcParameterFields.Limit] = 1
            };

            var args = new object[]
            {
                domain
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooProductConstants.Product,
                OdooRequestMethods.SearchRead,
                args,
                kwargs,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Array)
            {
                return new OdooProductInfo
                {
                    Id = 0,
                    Reference = reference
                };
            }

            var rows = result.Value.EnumerateArray().ToList();

            if (rows.Count == 0)
            {
                return new OdooProductInfo
                {
                    Id = 0,
                    Reference = reference
                };
            }

            var row = rows[0];

            return new OdooProductInfo
            {
                Id = (int)OdooJsonHelper.GetLong(row, OdooProductConstants.Fields.Id),
                Reference = OdooJsonHelper.GetString(row, OdooProductConstants.Fields.DefaultCode),
                Name = OdooJsonHelper.GetString(row, OdooProductConstants.Fields.Name),
                DisplayName = OdooJsonHelper.GetString(row, OdooProductConstants.Fields.DisplayName),
                ProductTemplateId = (int)OdooJsonHelper.GetMany2OneId(row, OdooProductConstants.Fields.ProductTemplateId),
                CategoryId = (int)OdooJsonHelper.GetMany2OneId(row, OdooProductConstants.Fields.CategoryId),
                CategoryName = OdooJsonHelper.GetMany2OneName(row, OdooProductConstants.Fields.CategoryId),
                UnitId = (int)OdooJsonHelper.GetMany2OneId(row, OdooProductConstants.Fields.UnitId),
                UnitName = OdooJsonHelper.GetMany2OneName(row, OdooProductConstants.Fields.UnitId)
            };
        }

        public async Task<OdooUnitInfo> GetUnitAsync(string unitName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(unitName))
            {
                return new OdooUnitInfo
                {
                    Name = string.Empty
                };
            }

            var normalizedUnit = unitName.Trim();

            var domain = new object[]
            {
                new object[]
                {
                    OdooProductConstants.UnitFields.Name,
                    "=",
                    normalizedUnit
                }
            };

            var fields = new object[]
            {
                OdooProductConstants.UnitFields.Id,
                OdooProductConstants.UnitFields.Name,
                OdooProductConstants.UnitFields.DisplayName,
                OdooProductConstants.UnitFields.Active
            };

            var kwargs = new Dictionary<string, object>
            {
                [OdooJsonRpcParameterFields.Fields] = fields,
                [OdooJsonRpcParameterFields.Limit] = 1
            };

            var args = new object[]
            {
                domain
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooProductConstants.UnitOfMeasure,
                OdooRequestMethods.SearchRead,
                args,
                kwargs,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Array)
            {
                return new OdooUnitInfo
                {
                    Name = normalizedUnit
                };
            }

            var rows = result.Value.EnumerateArray().ToList();

            if (rows.Count == 0)
            {
                return new OdooUnitInfo
                {
                    Name = normalizedUnit
                };
            }

            var row = rows[0];

            return new OdooUnitInfo
            {
                Id = (int)OdooJsonHelper.GetLong(row, OdooProductConstants.UnitFields.Id),
                Name = OdooJsonHelper.GetString(row, OdooProductConstants.UnitFields.Name),
                DisplayName = OdooJsonHelper.GetString(row, OdooProductConstants.UnitFields.DisplayName),
            };
        }

        public async Task<OdooCategoryInfo> GetCategoryAsync(string categoryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return new OdooCategoryInfo
                {
                    Name = string.Empty
                };
            }

            var normalizedCategory = categoryName.Trim();

            var domain = new object[]
            {
                new object[]
                {
                    OdooProductConstants.CategoryFields.CompleteName,
                    "=",
                    normalizedCategory
                }
            };

            var fields = new object[]
            {
                OdooProductConstants.CategoryFields.Id,
                OdooProductConstants.CategoryFields.Name,
                OdooProductConstants.CategoryFields.DisplayName,
                OdooProductConstants.CategoryFields.CompleteName
            };

            var kwargs = new Dictionary<string, object>
            {
                [OdooJsonRpcParameterFields.Fields] = fields,
                [OdooJsonRpcParameterFields.Limit] = 1
            };

            var args = new object[]
            {
                domain
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooProductConstants.Category,
                OdooRequestMethods.SearchRead,
                args,
                kwargs,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Array)
            {
                return new OdooCategoryInfo
                {
                    Name = normalizedCategory
                };
            }

            var rows = result.Value.EnumerateArray().ToList();

            if (rows.Count == 0)
            {
                return new OdooCategoryInfo
                {
                    Name = normalizedCategory
                };
            }

            var row = rows[0];

            return new OdooCategoryInfo
            {
                Id = (int)OdooJsonHelper.GetLong(row, OdooProductConstants.CategoryFields.Id),
                Name = OdooJsonHelper.GetString(row, OdooProductConstants.CategoryFields.Name),
                DisplayName = OdooJsonHelper.GetString(row, OdooProductConstants.CategoryFields.DisplayName),
                CompleteName = OdooJsonHelper.GetString(row, OdooProductConstants.CategoryFields.CompleteName)
            };
        }

        public async Task<OdooBomInfo> GetBomAsync(string productReference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productReference))
            {
                return new OdooBomInfo
                {
                    ProductReference = string.Empty
                };
            }

            var product = await GetProductAsync(productReference, cancellationToken);

            if (!product.Exists || product.ProductTemplateId <= 0)
            {
                return new OdooBomInfo
                {
                    ProductReference = productReference
                };
            }

            var domain = new object[]
            {
                new object[]
                {
                    OdooMrpConstants.BomFields.ProductTemplateId,
                    "=",
                    product.ProductTemplateId
                }
            };

            var fields = new object[]
            {
                OdooMrpConstants.BomFields.Id,
                OdooMrpConstants.BomFields.ProductId,
                OdooMrpConstants.BomFields.ProductTemplateId,
                OdooMrpConstants.BomFields.ProductQuantity,
                OdooMrpConstants.BomFields.ProductUnitId,
                OdooMrpConstants.BomFields.Code,
                OdooMrpConstants.BomFields.Type,
                OdooMrpConstants.BomFields.Active
            };

            var kwargs = new Dictionary<string, object>
            {
                [OdooJsonRpcParameterFields.Fields] = fields,
                [OdooJsonRpcParameterFields.Limit] = 1
            };

            var args = new object[]
            {
                domain
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.Bom,
                OdooRequestMethods.SearchRead,
                args,
                kwargs,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Array)
            {
                return new OdooBomInfo
                {
                    ProductReference = productReference,
                    ProductId = product.Id,
                    ProductTemplateId = product.ProductTemplateId
                };
            }

            var rows = result.Value.EnumerateArray().ToList();

            if (rows.Count == 0)
            {
                return new OdooBomInfo
                {
                    ProductReference = productReference,
                    ProductId = product.Id,
                    ProductTemplateId = product.ProductTemplateId
                };
            }

            var row = rows[0];

            return new OdooBomInfo
            {
                Id = (int)OdooJsonHelper.GetLong(row, OdooMrpConstants.BomFields.Id),
                ProductReference = productReference,

                ProductId = (int)OdooJsonHelper.GetMany2OneId(row, OdooMrpConstants.BomFields.ProductId),
                ProductTemplateId = (int)OdooJsonHelper.GetMany2OneId(row, OdooMrpConstants.BomFields.ProductTemplateId),

                Code = OdooJsonHelper.GetString(row, OdooMrpConstants.BomFields.Code),
                Type = OdooJsonHelper.GetString(row, OdooMrpConstants.BomFields.Type),

                ProductQuantity = OdooJsonHelper.GetDecimal(row, OdooMrpConstants.BomFields.ProductQuantity),

                UnitId = (int)OdooJsonHelper.GetMany2OneId(row, OdooMrpConstants.BomFields.ProductUnitId),
                UnitName = OdooJsonHelper.GetMany2OneName(row, OdooMrpConstants.BomFields.ProductUnitId)
            };
        }

        public async Task<OdooProductInfo> CreateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return new OdooProductInfo();

            if (!_executionOptions.AllowWrites)
            {
                return new OdooProductInfo
                {
                    Id = 0,
                    Reference = request.Reference,
                    Name = string.IsNullOrWhiteSpace(request.Name) ? request.Reference : request.Name,
                    CategoryId = request.CategoryId,
                    UnitId = request.UnitId
                };
            }

            var name = string.IsNullOrWhiteSpace(request.Name)
                ? request.Reference.Trim()
                : request.Name.Trim();

            var values = new Dictionary<string, object>
            {
                [OdooProductConstants.Fields.DefaultCode] = request.Reference.Trim(),
                [OdooProductConstants.Fields.Name] = name,
                [OdooProductConstants.Fields.Type] = OdooProductConstants.Types.Consumable,
                [OdooProductConstants.Fields.PurchaseOk] = request.CanBePurchased,
                [OdooProductConstants.Fields.SaleOk] = request.CanBeSold,
                [OdooProductConstants.Fields.IsStorable] = request.IsStorable
            };

            if (request.CategoryId > 0)
            {
                values[OdooProductConstants.Fields.CategoryId] = request.CategoryId;
            }

            if (request.UnitId <= 0)
            {
                throw new InvalidOperationException(
                    $"UnitId requerido para crear el producto '{request.Reference}'");
            }

            var args = new object[]
            {
                values
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooProductConstants.Product,
                OdooRequestMethods.Create,
                args,
                null,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Number)
                return new OdooProductInfo
                {
                    Reference = request.Reference
                };

            var createdId = result.Value.GetInt32();

            if (createdId <= 0)
                return new OdooProductInfo
                {
                    Reference = request.Reference
                };

            return await GetProductAsync(request.Reference, cancellationToken);
        }

        public Task<OdooProductInfo> CreateProductAsync(string reference, CancellationToken cancellationToken = default)
        {
            return CreateProductAsync(
                new OdooProductWriteRequest
                {
                    Reference = reference,
                    Name = reference
                },
                cancellationToken);
        }

        public async Task<OdooProductInfo> UpdateProductAsync(OdooProductWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reference))
                return new OdooProductInfo();

            var existing = await GetProductAsync(request.Reference, cancellationToken);

            if (!existing.Exists)
                return new OdooProductInfo { Reference = request.Reference };

            if (!_executionOptions.AllowWrites)
                return existing;

            var name = string.IsNullOrWhiteSpace(request.Name)
                ? request.Reference.Trim()
                : request.Name.Trim();

            var values = new Dictionary<string, object>
            {
                [OdooProductConstants.Fields.Name] = name,
                [OdooProductConstants.Fields.PurchaseOk] = request.CanBePurchased,
                [OdooProductConstants.Fields.SaleOk] = request.CanBeSold,
                [OdooProductConstants.Fields.IsStorable] = request.IsStorable
            };

            if (request.CategoryId > 0)
            {
                values[OdooProductConstants.Fields.CategoryId] = request.CategoryId;
            }

            if (request.UnitId > 0)
            {
                values[OdooProductConstants.Fields.UnitId] = request.UnitId;
            }

            var args = new object[]
            {
                new object[] { existing.Id },
                values
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooProductConstants.Product,
                OdooRequestMethods.Write,
                args,
                null,
                cancellationToken);

            if (result == null ||
                result.Value.ValueKind != JsonValueKind.True)
            {
                return existing;
            }

            return await GetProductAsync(request.Reference, cancellationToken);
        }

        public Task<OdooProductInfo> UpdateProductAsync(string reference, CancellationToken cancellationToken = default)
        {
            return UpdateProductAsync(
                new OdooProductWriteRequest
                {
                    Reference = reference,
                    Name = reference
                },
                cancellationToken);
        }

        public async Task<OdooBomInfo> CreateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductReference))
                return new OdooBomInfo();

            if (!_executionOptions.AllowWrites)
            {
                return new OdooBomInfo
                {
                    Id = 0,
                    ProductReference = request.ProductReference,
                    ProductId = request.ProductId,
                    ProductTemplateId = request.ProductTemplateId,
                    UnitId = request.UnitId,
                    ProductQuantity = request.ProductQuantity <= 0 ? 1m : request.ProductQuantity,
                    Code = request.Code,
                    Type = string.IsNullOrWhiteSpace(request.Type)
                        ? OdooMrpConstants.BomTypes.Normal
                        : request.Type
                };
            }

            if (request.ProductTemplateId <= 0)
                return new OdooBomInfo
                {
                    ProductReference = request.ProductReference
                };

            var values = new Dictionary<string, object>
            {
                [OdooMrpConstants.BomFields.ProductTemplateId] = request.ProductTemplateId,
                [OdooMrpConstants.BomFields.ProductQuantity] = request.ProductQuantity <= 0 ? 1m : request.ProductQuantity,
                [OdooMrpConstants.BomFields.Type] = string.IsNullOrWhiteSpace(request.Type)
                    ? OdooMrpConstants.BomTypes.Normal
                    : request.Type
            };

            if (request.ProductId > 0)
                values[OdooMrpConstants.BomFields.ProductId] = request.ProductId;

            if (request.UnitId > 0)
                values[OdooMrpConstants.BomFields.ProductUnitId] = request.UnitId;

            if (!string.IsNullOrWhiteSpace(request.Code))
                values[OdooMrpConstants.BomFields.Code] = request.Code.Trim();

            var args = new object[]
            {
                values
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.Bom,
                OdooRequestMethods.Create,
                args,
                null,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Number)
            {
                return new OdooBomInfo
                {
                    ProductReference = request.ProductReference,
                    ProductId = request.ProductId,
                    ProductTemplateId = request.ProductTemplateId
                };
            }

            var createdId = result.Value.GetInt32();

            if (createdId <= 0)
            {
                return new OdooBomInfo
                {
                    ProductReference = request.ProductReference,
                    ProductId = request.ProductId,
                    ProductTemplateId = request.ProductTemplateId
                };
            }

            return await GetBomAsync(request.ProductReference, cancellationToken);
        }

        public async Task<OdooBomInfo> CreateBomAsync(string productReference, CancellationToken cancellationToken = default)
        {
            var product = await GetProductAsync(productReference, cancellationToken);

            if (!product.Exists)
            {
                return new OdooBomInfo
                {
                    ProductReference = productReference
                };
            }

            return await CreateBomAsync(
                new OdooBomWriteRequest
                {
                    ProductReference = productReference,
                    ProductId = product.Id,
                    ProductTemplateId = product.ProductTemplateId,
                    UnitId = product.UnitId,
                    ProductQuantity = 1m,
                    Code = productReference,
                    Type = OdooMrpConstants.BomTypes.Normal
                },
                cancellationToken);
        }

        public async Task<OdooBomInfo> UpdateBomAsync(OdooBomWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductReference))
                return new OdooBomInfo();

            var existing = await GetBomAsync(request.ProductReference, cancellationToken);

            if (!existing.Exists)
            {
                return new OdooBomInfo
                {
                    ProductReference = request.ProductReference,
                    ProductId = request.ProductId,
                    ProductTemplateId = request.ProductTemplateId
                };
            }

            if (!_executionOptions.AllowWrites)
                return existing;

            var values = new Dictionary<string, object>
            {
                [OdooMrpConstants.BomFields.ProductQuantity] =
                    request.ProductQuantity <= 0 ? 1m : request.ProductQuantity,

                [OdooMrpConstants.BomFields.Type] =
                    string.IsNullOrWhiteSpace(request.Type)
                        ? OdooMrpConstants.BomTypes.Normal
                        : request.Type
            };

            if (request.ProductTemplateId > 0)
                values[OdooMrpConstants.BomFields.ProductTemplateId] = request.ProductTemplateId;

            if (request.ProductId > 0)
                values[OdooMrpConstants.BomFields.ProductId] = request.ProductId;

            if (request.UnitId > 0)
                values[OdooMrpConstants.BomFields.ProductUnitId] = request.UnitId;

            if (!string.IsNullOrWhiteSpace(request.Code))
                values[OdooMrpConstants.BomFields.Code] = request.Code.Trim();

            var args = new object[]
            {
                new object[] { existing.Id },
                values
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.Bom,
                OdooRequestMethods.Write,
                args,
                null,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.True)
                return existing;

            return await GetBomAsync(request.ProductReference, cancellationToken);
        }

        public async Task<OdooBomInfo> UpdateBomAsync(string productReference, CancellationToken cancellationToken = default)
        {
            var product = await GetProductAsync(productReference, cancellationToken);

            if (!product.Exists)
            {
                return new OdooBomInfo
                {
                    ProductReference = productReference
                };
            }

            return await UpdateBomAsync(
                new OdooBomWriteRequest
                {
                    ProductReference = productReference,
                    ProductId = product.Id,
                    ProductTemplateId = product.ProductTemplateId,
                    UnitId = product.UnitId,
                    ProductQuantity = 1m,
                    Code = productReference,
                    Type = OdooMrpConstants.BomTypes.Normal
                },
                cancellationToken);
        }

        public async Task<int> DeleteBomLinesAsync(int bomId, CancellationToken cancellationToken = default)
        {
            if (!_executionOptions.AllowWrites)
                return 0;

            if (bomId <= 0)
                return 0;

            var domain = new object[]
            {
                new object[]
                {
                    OdooMrpConstants.BomLineFields.BomId,
                    "=",
                    bomId
                }
            };

            var searchArgs = new object[]
            {
                domain
            };

            var searchResult = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.BomLine,
                OdooRequestMethods.Search,
                searchArgs,
                null,
                cancellationToken);

            if (searchResult == null || searchResult.Value.ValueKind != JsonValueKind.Array)
                return 0;

            var ids = searchResult.Value
                .EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.Number)
                .Select(x => x.GetInt32())
                .Where(x => x > 0)
                .ToArray();

            if (ids.Length == 0)
                return 0;

            var unlinkArgs = new object[]
            {
                ids
            };

            var unlinkResult = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.BomLine,
                OdooRequestMethods.Unlink,
                unlinkArgs,
                null,
                cancellationToken);

            if (unlinkResult == null || unlinkResult.Value.ValueKind != JsonValueKind.True)
                return 0;

            return ids.Length;
        }

        public async Task<int> CreateBomLineAsync(OdooBomLineWriteRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return 0;

            if (!_executionOptions.AllowWrites)
                return 0;

            if (request.BomId <= 0 || request.ProductId <= 0)
                return 0;

            var values = new Dictionary<string, object>
            {
                [OdooMrpConstants.BomLineFields.BomId] = request.BomId,
                [OdooMrpConstants.BomLineFields.ProductId] = request.ProductId,
                [OdooMrpConstants.BomLineFields.ProductQuantity] = request.Quantity <= 0 ? 1m : request.Quantity,
                [OdooMrpConstants.BomLineFields.Sequence] = request.Sequence
            };

            if (request.UnitId > 0)
                values[OdooMrpConstants.BomLineFields.ProductUnitId] = request.UnitId;

            var args = new object[]
            {
                values
            };

            var result = await _client.ExecuteKwAsync(
                _session.Database,
                _session.UserId,
                _session.Password,
                OdooMrpConstants.BomLine,
                OdooRequestMethods.Create,
                args,
                null,
                cancellationToken);

            if (result == null || result.Value.ValueKind != JsonValueKind.Number)
                return 0;

            return result.Value.GetInt32();
        }

        public async Task<int> ReplaceBomLinesAsync(int bomId, IReadOnlyCollection<OdooBomLineWriteRequest> lines, CancellationToken cancellationToken = default)
        {
            if (bomId <= 0)
                return 0;

            if (!_executionOptions.AllowWrites)
                return lines?.Count ?? 0;

            await DeleteBomLinesAsync(bomId, cancellationToken);

            if (lines == null || lines.Count == 0)
                return 0;

            var createdLines = 0;
            var sequence = 10;

            foreach (var line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (line == null)
                    continue;

                line.BomId = bomId;

                if (line.Sequence <= 0)
                {
                    line.Sequence = sequence;
                    sequence += 10;
                }

                var lineId = await CreateBomLineAsync(line, cancellationToken);

                if (lineId > 0)
                    createdLines++;
            }

            return createdLines;
        }

        public async Task<int> SyncBomLinesAsync(string productReference, IReadOnlyCollection<OdooBomLineWriteRequest> lines, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productReference))
                return 0;

            var bom = await GetBomAsync(productReference, cancellationToken);

            if (!bom.Exists)
                return 0;

            if (!_executionOptions.AllowWrites)
                return lines?.Count ?? 0;

            return await ReplaceBomLinesAsync(bom.Id, lines ?? Array.Empty<OdooBomLineWriteRequest>(), cancellationToken);
        }
    }
}