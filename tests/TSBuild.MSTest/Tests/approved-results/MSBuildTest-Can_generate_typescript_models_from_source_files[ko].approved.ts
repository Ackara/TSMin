export class Lion {
	name: KnockoutObservable<string>;
	legs: KnockoutObservable<number>;
	marketPrice: KnockoutObservable<number>;
	extinct: KnockoutObservable<boolean>;
	discoveryDate: any;
	
	copy(): void{}
}
