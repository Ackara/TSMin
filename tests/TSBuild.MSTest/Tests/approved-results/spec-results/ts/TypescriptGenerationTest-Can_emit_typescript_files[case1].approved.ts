namespace App {
	export interface IBasicInfo {
		firstName?: string;
		lastName?: string;
		address?: string;
		state?: string;
		city?: string;
		zipcode?: string;
	}

	export class Form720 implements IBasicInfo {
		gross: number;
		payments: number;
		firstName: string;
		lastName: string;
		address: string;
		state: string;
		city: string;
		zipcode: string;
	}
}
