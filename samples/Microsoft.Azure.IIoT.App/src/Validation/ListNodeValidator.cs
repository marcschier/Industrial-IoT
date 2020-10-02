﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Validation {
    using FluentValidation;
    using Microsoft.Azure.IIoT.App.Models;

    public class ListNodeValidator : AbstractValidator<ListNodeRequested> {

        public ListNodeValidator() {
            RuleFor(p => p.RequestedPublishingInterval)
                .Must(BeAValidIntervalMs)
                .WithMessage("Publishing interval must be a number greater than 0 ms.");

            RuleFor(p => p.RequestedSamplingInterval)
                .Must(BeAValidIntervalMs)
                .WithMessage("Sampling interval must be a number greater than 0 ms.");

            RuleFor(p => p.RequestedHeartbeatInterval)
                .Must(BeAValidIntervalSec)
                .WithMessage("Heartbeat interval must be a number greater than 0 second.");
        }

        private bool BeAValidIntervalMs(string value) {
            if (ValidationUtils.ShouldUseDefaultValue(value)) {
                return true;
            }

            if (double.TryParse(value, out var result)) {
                return result > 0;
            }

            return false;
        }

        private bool BeAValidIntervalSec(string value) {
            if (ValidationUtils.ShouldUseDefaultValue(value)) {
                return true;
            }

            if (double.TryParse(value, out var result)) {
                return result > 0;
            }

            return false;
        }
    }
}
