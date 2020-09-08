/// <reference path="../../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export class Contact1Model {
		constructor(model?: any) {
			this.firstName = ko.observable((model && model.hasOwnProperty('firstName'))? model.firstName : null);
			this.lastName = ko.observable((model && model.hasOwnProperty('lastName'))? model.lastName : null);
		}

		public copy(model: any) {
			this.firstName((model && model.hasOwnProperty('firstName'))? model.firstName : null);
			this.lastName((model && model.hasOwnProperty('lastName'))? model.lastName : null);
		}

		firstName: KnockoutObservable<string>;
		lastName: KnockoutObservable<string>;
	}

	export class Form100Model extends Contact1Model {
		constructor(model?: any) {
			super(model);
			this.age = ko.observable((model && model.hasOwnProperty('age'))? model.age : null);
			this.gross = ko.observable((model && model.hasOwnProperty('gross'))? model.gross : null);
		}

		public copy(model: any) {
			this.age((model && model.hasOwnProperty('age'))? model.age : null);
			this.gross((model && model.hasOwnProperty('gross'))? model.gross : null);
		}

		age: KnockoutObservable<number>;
		gross: KnockoutObservable<number>;
	}
}
