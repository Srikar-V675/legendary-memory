using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Service.Interface;

namespace BidSphere.Service.Implementation
{
    public class AsqlParserService : IAsqlParserService
    {
        public IQueryable<Product> ApplyAsqlFilter(IQueryable<Product> query, string asqlQuery)
        {
            if (string.IsNullOrWhiteSpace(asqlQuery))
            {
                return query;
            }

            // Split by OR first
            var orParts = SplitByLogicalOperator(asqlQuery, " OR ");

            IQueryable<Product>? combinedQuery = null;

            foreach (var orPart in orParts)
            {
                var tempQuery = query;

                // Split by AND
                var andParts = SplitByLogicalOperator(orPart, " AND ");

                foreach (var condition in andParts)
                {
                    tempQuery = ApplyCondition(tempQuery, condition.Trim());
                }

                // Combine with OR logic
                if (combinedQuery == null)
                {
                    combinedQuery = tempQuery;
                }
                else
                {
                    combinedQuery = combinedQuery.Union(tempQuery);
                }
            }

            return combinedQuery ?? query;
        }

        private List<string> SplitByLogicalOperator(string query, string op)
        {
            var parts = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < query.Length; i++)
            {
                if (query[i] == '"')
                {
                    inQuotes = !inQuotes;
                    current += query[i];
                }
                else if (!inQuotes && i <= query.Length - op.Length && query.Substring(i, op.Length) == op)
                {
                    parts.Add(current);
                    current = "";
                    i += op.Length - 1;
                }
                else
                {
                    current += query[i];
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current);
            }

            return parts;
        }

        private IQueryable<Product> ApplyCondition(IQueryable<Product> query, string condition)
        {
            try
            {
                // Handle "in" operator
                if (condition.Contains(" in "))
                {
                    return ApplyInCondition(query, condition);
                }

                // Parse field, operator, value
                string field, op, value;

                if (condition.Contains("!="))
                {
                    var parts = condition.Split("!=", 2);
                    field = parts[0].Trim().ToLower();
                    op = "!=";
                    value = parts[1].Trim();
                }
                else if (condition.Contains(">="))
                {
                    var parts = condition.Split(">=", 2);
                    field = parts[0].Trim().ToLower();
                    op = ">=";
                    value = parts[1].Trim();
                }
                else if (condition.Contains("<="))
                {
                    var parts = condition.Split("<=", 2);
                    field = parts[0].Trim().ToLower();
                    op = "<=";
                    value = parts[1].Trim();
                }
                else if (condition.Contains(">"))
                {
                    var parts = condition.Split(">", 2);
                    field = parts[0].Trim().ToLower();
                    op = ">";
                    value = parts[1].Trim();
                }
                else if (condition.Contains("<"))
                {
                    var parts = condition.Split("<", 2);
                    field = parts[0].Trim().ToLower();
                    op = "<";
                    value = parts[1].Trim();
                }
                else if (condition.Contains("="))
                {
                    var parts = condition.Split("=", 2);
                    field = parts[0].Trim().ToLower();
                    op = "=";
                    value = parts[1].Trim();
                }
                else
                {
                    return query;
                }

                // Remove quotes from value
                value = value.Trim('"');

                // Apply filter based on field
                return field switch
                {
                    "productid" => ApplyNumericFilter(query, p => p.ProductId, op, value),
                    "name" => ApplyStringFilter(query, p => p.Name, op, value),
                    "category" => ApplyStringFilter(query, p => p.Category, op, value),
                    "startingprice" => ApplyDecimalFilter(query, p => p.StartingPrice, op, value),
                    "status" => ApplyStatusFilter(query, op, value),
                    _ => query
                };
            }
            catch
            {
                return query;
            }
        }

        private IQueryable<Product> ApplyInCondition(IQueryable<Product> query, string condition)
        {
            var parts = condition.Split(" in ", 2);
            var field = parts[0].Trim().ToLower();
            var arrayStr = parts[1].Trim().Trim('[', ']');

            var values = arrayStr.Split(',')
                .Select(v => v.Trim().Trim('"'))
                .ToList();

            return field switch
            {
                "category" => query.Where(p => values.Contains(p.Category)),
                "name" => query.Where(p => values.Contains(p.Name)),
                _ => query
            };
        }

        private IQueryable<Product> ApplyNumericFilter(IQueryable<Product> query, Func<Product, int> selector, string op, string value)
        {
            if (!int.TryParse(value, out var numValue))
                return query;

            return op switch
            {
                "=" => query.Where(p => selector(p) == numValue),
                "!=" => query.Where(p => selector(p) != numValue),
                ">" => query.Where(p => selector(p) > numValue),
                ">=" => query.Where(p => selector(p) >= numValue),
                "<" => query.Where(p => selector(p) < numValue),
                "<=" => query.Where(p => selector(p) <= numValue),
                _ => query
            };
        }

        private IQueryable<Product> ApplyDecimalFilter(IQueryable<Product> query, Func<Product, decimal> selector, string op, string value)
        {
            if (!decimal.TryParse(value, out var numValue))
                return query;

            return op switch
            {
                "=" => query.Where(p => selector(p) == numValue),
                "!=" => query.Where(p => selector(p) != numValue),
                ">" => query.Where(p => selector(p) > numValue),
                ">=" => query.Where(p => selector(p) >= numValue),
                "<" => query.Where(p => selector(p) < numValue),
                "<=" => query.Where(p => selector(p) <= numValue),
                _ => query
            };
        }

        private IQueryable<Product> ApplyStringFilter(IQueryable<Product> query, Func<Product, string> selector, string op, string value)
        {
            return op switch
            {
                "=" => query.Where(p => selector(p) == value),
                "!=" => query.Where(p => selector(p) != value),
                _ => query
            };
        }

        private IQueryable<Product> ApplyStatusFilter(IQueryable<Product> query, string op, string value)
        {
            if (!Enum.TryParse<AuctionStatus>(value, true, out var status))
                return query;

            return op switch
            {
                "=" => query.Where(p => p.Auction != null && p.Auction.Status == status),
                "!=" => query.Where(p => p.Auction != null && p.Auction.Status != status),
                _ => query
            };
        }
    }
}
