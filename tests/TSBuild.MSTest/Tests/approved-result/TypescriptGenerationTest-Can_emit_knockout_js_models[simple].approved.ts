/// <reference path="../../../node_modules/@types/knockout/index.d.ts" />
namespace App {
	export abstract class Animal {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs = ko.observable((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status = ko.observable((model && model.hasOwnProperty('status'))? model.status : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status((model && model.hasOwnProperty('status'))? model.status : null);
		}

		name: KnockoutObservable<string>;
		legs: KnockoutObservable<number>;
		status: KnockoutObservable<any>;
	}
}
