/// <reference path="../../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export interface IJobInfoModel {
		id?: KnockoutObservable<string>;
		name?: KnockoutObservable<string>;
	}
}
