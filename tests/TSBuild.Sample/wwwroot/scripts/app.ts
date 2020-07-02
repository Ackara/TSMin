/// <reference path="../../node_modules/@types/knockout/index.d.ts" />

export class Contact {
	constructor(model?: any) {
		this.id = ko.observable((model && model.hasOwnProperty('id'))? model.id : null);
		this.email = ko.observable((model && model.hasOwnProperty('email'))? model.email : null);
		this.regisitrationDate = ko.observable((model && model.hasOwnProperty('regisitrationDate'))? model.regisitrationDate : null);
	}

	public copy(model: any) {
		this.id((model && model.hasOwnProperty('id'))? model.id : null);
		this.email((model && model.hasOwnProperty('email'))? model.email : null);
		this.regisitrationDate((model && model.hasOwnProperty('regisitrationDate'))? model.regisitrationDate : null);
	}

	id: KnockoutObservable<string>;
	email: KnockoutObservable<string>;
	regisitrationDate: any;
}
