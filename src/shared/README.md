### shared

In this subfolder live all code files to be included from projects with different native compilation targets, a.k.a. shared code.

Do provide `.props` files to prescribe file ordering of shared code for easy inclusion in projects.

Subfolder structure:

* `shared/<semantic_name>` : contains unions, records, functions and classes that capture the shared code part of any kind of thing.
* `shared/<semantic_name>/widgets` : contains the shared code part of your UI widgets. Just the rendering part has to be target-specific. The model + message protocol type and update logic of the widgets can be captured by records and unions in platform-independent manner.