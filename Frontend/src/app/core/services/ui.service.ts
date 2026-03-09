import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class UiService {
    private openModalSource = new Subject<string>();
    openModal$ = this.openModalSource.asObservable();

    triggerModal(modalName: string) {
        this.openModalSource.next(modalName);
    }
}
