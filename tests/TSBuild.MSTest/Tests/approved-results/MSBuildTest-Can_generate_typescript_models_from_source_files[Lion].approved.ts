export interface IAnimal {
	name?: string;
	legs?: number;
	marketPrice?: number;
	extinct?: boolean;
	discoveryDate?: any;
}

export class Lion implements IAnimal {
	name: string;
	legs: number;
	marketPrice: number;
	extinct: boolean;
	discoveryDate: any;
}
