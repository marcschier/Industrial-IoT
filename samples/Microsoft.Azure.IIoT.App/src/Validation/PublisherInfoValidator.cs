// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Validation {
    using FluentValidation;
    using Microsoft.Azure.IIoT.App.Models;
    using System;
    using System.Diagnostics;

    public class PublisherInfoValidator : AbstractValidator<PublisherInfoRequested> {

        private static readonly ValidationUtils kUtils = new ValidationUtils();

        public PublisherInfoValidator() {
            RuleFor(p => p.RequestedLogLevel)
                .Must(BeTraceLevel)
                .WithMessage("Must be a trace level value.");
        }

        private bool BeTraceLevel(string value) {
            if (kUtils.ShouldUseDefaultValue(value)) {
                return true;
            }

            if (Enum.TryParse<TraceLevel>(value, out _)) {
                return true;
            }

            return false;
        }
    }
}
