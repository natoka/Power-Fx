﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerFx.Core.Binding;
using Microsoft.PowerFx.Core.Logging.Trackers;
using Microsoft.PowerFx.Core.Syntax;
using Microsoft.PowerFx.Core.Syntax.Nodes;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;

namespace Microsoft.PowerFx.Core.Functions.FunctionArgValidators
{
    internal sealed class SortOrderValidator : IArgValidator<string>
    {
        public bool TryGetValidValue(TexlNode argNode, TexlBinding binding, out string validatedOrder)
        {
            return TryGetValidSortOrder(argNode, binding, out validatedOrder);
        }

        private bool TryGetValidSortOrder(TexlNode argNode, TexlBinding binding, out string validatedOrder)
        {
            Contracts.AssertValue(argNode);
            Contracts.AssertValue(binding);

            validatedOrder = "";
            if (binding.ErrorContainer.HasErrors(argNode))
                return false;

            switch (argNode.Kind)
            {
            case NodeKind.FirstName:
                return TryGetValidSortOrderNode(argNode.AsFirstName(), binding, out validatedOrder);
            case NodeKind.DottedName:
                return TryGetValidSortOrderNode(argNode.AsDottedName(), binding, out validatedOrder);
            case NodeKind.StrLit:
                return TryGetValidSortOrderNode(argNode.AsStrLit(), out validatedOrder);
            default:
                TrackingProvider.Instance.AddSuggestionMessage("Invalid sortorder node type", argNode, binding);
                return false;
            }
        }

        private bool IsValidOrderString(string order, out string validatedSortOrder)
        {
            Contracts.AssertValue(order);

            validatedSortOrder = "";
            order = order.ToLower();
            if (order != LanguageConstants.AscendingSortOrderString && order != LanguageConstants.DescendingSortOrderString)
                return false;

            validatedSortOrder = order;
            return true;
        }

        private bool TryGetValidSortOrderNode(DottedNameNode node, TexlBinding binding, out string sortOrder)
        {
            Contracts.AssertValue(node);
            Contracts.AssertValue(binding);

            sortOrder = "";
            TexlNode lhsNode = node.Left;
            var orderEnum = lhsNode.AsFirstName();
            if (orderEnum == null)
                return false;

            // Verify order enum
            if (!VerifyFirstNameNodeIsValidSortOrderEnum(orderEnum, binding))
                return false;

            string order = node.Right.Name.Value;
            return IsValidOrderString(order, out sortOrder);
        }

        private bool TryGetValidSortOrderNode(FirstNameNode node, TexlBinding binding, out string sortOrder)
        {
            Contracts.AssertValue(node);
            Contracts.AssertValue(binding);

            sortOrder = "";
            var info = binding.GetInfo(node).VerifyValue();
            if (info.Kind != BindKind.Enum)
                return false;

            string order = info.Data as string;
            if (order == null)
                return false;

            return IsValidOrderString(order, out sortOrder);
        }

        private bool TryGetValidSortOrderNode(StrLitNode node, out string sortOrder)
        {
            Contracts.AssertValue(node);

            string order = node.Value;
            return IsValidOrderString(order, out sortOrder);
        }

        private bool VerifyFirstNameNodeIsValidSortOrderEnum(FirstNameNode node, TexlBinding binding)
        {
            Contracts.AssertValue(node);
            Contracts.AssertValue(binding);

            var firstNameInfo = binding.GetInfo(node);
            if (firstNameInfo == null || firstNameInfo.Kind != BindKind.Enum)
                return false;


            if (!binding.NameResolver.TryLookupEnum(new DName(LanguageConstants.SortOrderEnumStringInvariant), out var lookupInfo))
                return false;

            DType type = binding.GetType(node);

            return type == lookupInfo.Type;
        }
    }
}
