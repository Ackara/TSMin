/// <reference path="../../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export interface IBasicInfoModel {
		firstName?: KnockoutObservable<string>;
		lastName?: KnockoutObservable<string>;
		address?: KnockoutObservable<string>;
		state?: KnockoutObservable<string>;
		city?: KnockoutObservable<string>;
		zipcode?: KnockoutObservable<string>;
	}

	export class Form720Model implements IBasicInfoModel {
		constructor(model?: any) {
			this.gross = ko.observable((model && model.hasOwnProperty('gross'))? model.gross : null);
			this.payments = ko.observable((model && model.hasOwnProperty('payments'))? model.payments : null);
			this.firstName = ko.observable((model && model.hasOwnProperty('firstName'))? model.firstName : null);
			this.lastName = ko.observable((model && model.hasOwnProperty('lastName'))? model.lastName : null);
			this.address = ko.observable((model && model.hasOwnProperty('address'))? model.address : null);
			this.state = ko.observable((model && model.hasOwnProperty('state'))? model.state : null);
			this.city = ko.observable((model && model.hasOwnProperty('city'))? model.city : null);
			this.zipcode = ko.observable((model && model.hasOwnProperty('zipcode'))? model.zipcode : null);
		}

		public copy(model: any) {
			this.gross((model && model.hasOwnProperty('gross'))? model.gross : null);
			this.payments((model && model.hasOwnProperty('payments'))? model.payments : null);
			this.firstName((model && model.hasOwnProperty('firstName'))? model.firstName : null);
			this.lastName((model && model.hasOwnProperty('lastName'))? model.lastName : null);
			this.address((model && model.hasOwnProperty('address'))? model.address : null);
			this.state((model && model.hasOwnProperty('state'))? model.state : null);
			this.city((model && model.hasOwnProperty('city'))? model.city : null);
			this.zipcode((model && model.hasOwnProperty('zipcode'))? model.zipcode : null);
		}

		gross: KnockoutObservable<number>;
		payments: KnockoutObservable<number>;
		firstName: KnockoutObservable<string>;
		lastName: KnockoutObservable<string>;
		address: KnockoutObservable<string>;
		state: KnockoutObservable<string>;
		city: KnockoutObservable<string>;
		zipcode: KnockoutObservable<string>;
	}
}
