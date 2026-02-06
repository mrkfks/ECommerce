import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[ngLet]',
  standalone: true
})
export class LetDirective {
  @Input()
  set ngLet(value: any) {
    this.viewContainer.clear();
    this.viewContainer.createEmbeddedView(this.templateRef, {
      $implicit: value
    });
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef
  ) {}
}
